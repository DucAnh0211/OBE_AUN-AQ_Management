using System.Data;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace ObeAunQa.SharedKernel;

// The column is deliberately present on every tenant table, including read models.
// Database foreign keys and this write guard prevent a forged foreign-school parent.
public static class SchoolData
{
    public static void Configure(ModelBuilder model, DbContext context, params string[] excluded)
    {
        foreach (var type in model.Model.GetEntityTypes().ToArray())
        {
            if (excluded.Contains(type.ClrType.Name) || type.GetViewName() != null) continue;
            var university = type.GetTableName() == "universities";
            var property = university ? "Id" : type.ClrType.Name == "User" ? "UniversityId" : "SchoolId";
            if (!university) model.Entity(type.ClrType).Property<long?>(property).HasColumnName("school_id");
            if (!university) model.Entity(type.ClrType).HasIndex(property);
            var row = Expression.Parameter(type.ClrType, "row");
            var scope = Expression.Property(Expression.Constant(context), "ScopeSchoolId");
            var bypass = Expression.Property(Expression.Constant(context), "ScopeBypass");
            Expression value = Expression.Call(typeof(EF), nameof(EF.Property), [university ? typeof(long) : typeof(long?)], row, Expression.Constant(property));
            if (university) value = Expression.Convert(value, typeof(long?));
            model.Entity(type.ClrType).HasQueryFilter("School", Expression.Lambda(Expression.OrElse(bypass,
                Expression.AndAlso(Expression.NotEqual(scope, Expression.Constant(null, typeof(long?))), Expression.Equal(value, scope))), row));
        }
    }

    public static async Task GuardAsync(DbContext db, ICurrentUser? user, CancellationToken ct)
    {
        var entries = db.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray();
        var system = user == null || user.IsSystemContext;
        if (!system && user?.UniversityId is long selected) {
            if (!await db.Database.SqlQueryRaw<bool>("SELECT EXISTS(SELECT 1 FROM curriculum.universities WHERE id={0} AND archived_at IS NULL) AS \"Value\"",selected).SingleAsync(ct)) throw Denied();
        }
        foreach (var entry in entries)
        {
            var property = entry.Metadata.FindProperty("SchoolId");
            if (property == null) continue;
            if (entry.Metadata.ClrType.Name is "AuditLog" or "CurriculumAuditRecord") continue;
            long? school = entry.Property(property.Name).CurrentValue as long?;
            var proposed = entry.State == EntityState.Added;
            if (proposed) school ??= user?.UniversityId;
            var actual = entry.Metadata.FindProperty("UniversityId");
            if (actual != null) Merge(entry.Property(actual.Name).CurrentValue as long?);
            foreach (var fk in entry.Metadata.GetForeignKeys())
            {
                if (fk.Properties.Any(p => p.Name is "CreatedBy" or "UpdatedBy" or "ActorId" or "AssignedBy" or "SubmittedBy" or "PublishedBy" or "DisabledBy")) continue;
                var parent = fk.PrincipalEntityType;
                var tenant = parent.FindProperty("SchoolId") ?? (parent.GetTableName() == "universities" ? parent.FindProperty("Id") : parent.ClrType.Name == "User" ? parent.FindProperty("UniversityId") : null);
                if (tenant == null) continue;
                var values = fk.Properties.Select(p => entry.Property(p.Name).CurrentValue).ToArray();
                if (values.Any(v => v == null)) continue;
                var tracked = db.ChangeTracker.Entries().FirstOrDefault(e => e.Metadata == parent && e.State != EntityState.Deleted
                    && fk.PrincipalKey.Properties.Select(p => e.Property(p.Name).CurrentValue).SequenceEqual(values));
                long? parentSchool;
                if (tracked != null) parentSchool = tracked.Property(tenant.Name).CurrentValue as long?;
                else parentSchool = await ParentSchool(db, parent, tenant, fk.PrincipalKey.Properties, values, ct);
                if (parentSchool == null && !system) throw Denied();
                Merge(parentSchool);
            }
            // These references intentionally cross module boundaries without EF navigations.
            foreach (var reference in new[] {
                (Field:"UserId",Schema:"security",Table:"users",Key:"id"),
                (Field:"OwnerUserId",Schema:"security",Table:"users",Key:"id"),
                (Field:"OwnerId",Schema:"security",Table:"users",Key:"id"),
                (Field:"RequestedBy",Schema:"security",Table:"users",Key:"id"),
                (Field:"ProgramId",Schema:"curriculum",Table:"programs",Key:"id"),
                (Field:"VersionId",Schema:"curriculum",Table:"program_versions",Key:"id"),
                (Field:"ProgramCourseId",Schema:"curriculum",Table:"program_courses",Key:"id"),
                (Field:"DocumentId",Schema:"accreditation",Table:"source_documents",Key:"Id") })
            {
                var field = entry.Metadata.FindProperty(reference.Field);
                if (field == null || reference.Field == "DocumentId" && field.ClrType != typeof(Guid)
                    || entry.Metadata.GetForeignKeys().Any(f => f.Properties.Contains(field))) continue;
                var value = entry.Property(field.Name).CurrentValue;
                if(value == null) continue;
                var connection = db.Database.GetDbConnection();
                if(connection.State != ConnectionState.Open) await connection.OpenAsync(ct);
                await using var command = connection.CreateCommand(); command.Transaction=db.Database.CurrentTransaction?.GetDbTransaction();
                command.CommandText=$"SELECT school_id FROM \"{reference.Schema}\".\"{reference.Table}\" WHERE \"{reference.Key}\"=@id";
                var parameter=command.CreateParameter();parameter.ParameterName="id";parameter.Value=value;command.Parameters.Add(parameter);
                var found=await command.ExecuteScalarAsync(ct);
                var parentSchool=found is null or DBNull ? (long?)null : Convert.ToInt64(found);
                // A global administrator may own a school-specific conversation.
                if(parentSchool==null && reference.Field=="OwnerId" && user?.Role==SystemRoles.Admin) continue;
                if(parentSchool==null && !system) throw Denied();
                Merge(parentSchool);
            }
            if (!system && (school == null || user?.UniversityId != null && school != user.UniversityId
                || user?.Role != SystemRoles.Admin && school != user?.UniversityId)) throw Denied();
            if (!proposed && !Equals(entry.Property(property.Name).OriginalValue, school)) throw Denied();
            entry.Property(property.Name).CurrentValue = school;
            void Merge(long? value) { if (value == null) return; if (school != null && school != value) throw Denied(); school = value; }
        }
    }

    private static async Task<long?> ParentSchool(DbContext db, IReadOnlyEntityType parent, IReadOnlyProperty tenant,
        IReadOnlyList<IReadOnlyProperty> keys, object?[] values, CancellationToken ct)
    {
        var table = StoreObjectIdentifier.Table(parent.GetTableName()!, parent.GetSchema());
        static string Q(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = $"SELECT {Q(tenant.GetColumnName(table)!)} FROM {Q(table.Schema!)}.{Q(table.Name)} WHERE "
            + string.Join(" AND ", keys.Select((p, i) => Q(p.GetColumnName(table)!) + " = @p" + i));
        for (var i = 0; i < values.Length; i++) { var p = command.CreateParameter(); p.ParameterName = "p" + i; p.Value = values[i]!; command.Parameters.Add(p); }
        var result = await command.ExecuteScalarAsync(ct);
        return result is null or DBNull ? null : Convert.ToInt64(result);
    }
    private static SchoolAccessException Denied() => new();
}
