using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

public partial class CurriculumDbContext : DbContext
{
    private readonly ObeAunQa.SharedKernel.ICurrentUser? currentUser;
    public CurriculumDbContext(DbContextOptions<CurriculumDbContext> options, ObeAunQa.SharedKernel.ICurrentUser? currentUser = null)
        : base(options)
    {
        this.currentUser = currentUser;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();
        await ObeAunQa.SharedKernel.SchoolData.GuardAsync(this, currentUser, cancellationToken);
        var changed = ChangeTracker.Entries().Where(e => e.Entity is not CurriculumAuditRecord && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray();
        if (currentUser is null || changed.Length == 0) return await base.SaveChangesAsync(cancellationToken);
        var logs = changed.Select(e => new CurriculumAuditRecord { ActorId = currentUser.UserId, CreatedAt = DateTime.UtcNow,
            Entity = e.Metadata.ClrType.Name, Action = e.State.ToString(), Changes = System.Text.Json.JsonSerializer.Serialize(e.Properties
                .Where(p => p.IsModified || p.Metadata.IsPrimaryKey() || e.State != EntityState.Modified).Select(p => new { field = p.Metadata.Name,
                    before = e.State == EntityState.Added ? null : p.OriginalValue,
                    after = e.State == EntityState.Deleted ? null : p.CurrentValue })) }).ToArray();
        var own = Database.CurrentTransaction == null ? await Database.BeginTransactionAsync(cancellationToken) : null;
        try {
            var result = await base.SaveChangesAsync(cancellationToken);
            for(var i=0;i<logs.Length;i++) logs[i].Changes=System.Text.Json.JsonSerializer.Serialize(new {
                key=changed[i].Properties.Where(p=>p.Metadata.IsPrimaryKey()).ToDictionary(p=>p.Metadata.Name,p=>p.CurrentValue),
                properties=System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(logs[i].Changes)
            });
            AddRange(logs);
            for(var i=0;i<logs.Length;i++) Entry(logs[i]).Property("SchoolId").CurrentValue = changed[i].Metadata.ClrType.Name=="University"
                ? changed[i].Property("Id").CurrentValue : changed[i].Metadata.FindProperty("SchoolId")!=null ? changed[i].Property("SchoolId").CurrentValue : currentUser.UniversityId;
            await base.SaveChangesAsync(cancellationToken);
            if (own != null) await own.CommitAsync(cancellationToken);
            return result;
        } finally { if (own != null) await own.DisposeAsync(); }
    }

    public virtual DbSet<Clo> Clos { get; set; }

    public virtual DbSet<CloPlo> CloPlos { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CoursePlo> CoursePlos { get; set; }

    public virtual DbSet<ImportBatch> ImportBatches { get; set; }

    public virtual DbSet<Plo> Plos { get; set; }

    public virtual DbSet<PloBalance> PloBalances { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<ProgramCourse> ProgramCourses { get; set; }

    public virtual DbSet<ProgramVersion> ProgramVersions { get; set; }

    public virtual DbSet<ProgressionCode> ProgressionCodes { get; set; }

    public virtual DbSet<SourceCourseRow> SourceCourseRows { get; set; }

    public virtual DbSet<SourceDocument> SourceDocuments { get; set; }

    public virtual DbSet<University> Universities { get; set; }

    public virtual DbSet<WeightCode> WeightCodes { get; set; }

    public long? ScopeSchoolId => currentUser?.UniversityId;
    public bool ScopeBypass => currentUser == null || currentUser.IsSystemContext || currentUser.Role == "admin" && currentUser.UniversityId == null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("course_offering_codes", "curriculum").StartsAt(100000).IncrementsBy(1).HasMax(999999);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CurriculumDbContext).Assembly,
            type => type.Namespace == "ObeAunQa.Modules.Curriculum.Infrastructure");
        ObeAunQa.SharedKernel.SchoolData.Configure(modelBuilder, this, "ProgressionCode", "WeightCode", "PloBalance");
    }

}
