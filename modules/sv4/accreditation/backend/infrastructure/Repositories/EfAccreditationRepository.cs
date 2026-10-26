using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;

internal sealed class EfAccreditationRepository(AccreditationDbContext db) : IAccreditationRepository
{
    private IQueryable<AunFramework> Graph => db.AunFrameworks
        .Include(f=>f.AunCriteria).ThenInclude(c=>c.AunCriterionTranslations)
        .Include(f=>f.AunCriteria).ThenInclude(c=>c.AunRequirements).ThenInclude(r=>r.AunRequirementTranslations);
    public async Task<IReadOnlyList<AunFramework>> ListAsync(string? status,string? search,CancellationToken ct) =>
        await Graph.AsNoTracking().Where(f=>(status==null || f.Status==status) && (search==null || EF.Functions.ILike(f.Code,"%"+search+"%") || EF.Functions.ILike(f.Version,"%"+search+"%") || EF.Functions.ILike(f.SourceTitle,"%"+search+"%")))
            .OrderByDescending(f=>f.CreatedAt).ThenByDescending(f=>f.Id).ToListAsync(ct);
    public Task<AunFramework?> FindAsync(long id,bool tracking,CancellationToken ct) => (tracking?Graph:Graph.AsNoTracking()).SingleOrDefaultAsync(f=>f.Id==id,ct);
    public void Add(AunFramework framework) => db.AunFrameworks.Add(framework);
    public void Remove(AunFramework framework) => db.AunFrameworks.Remove(framework);
    public async Task<T> WriteAsync<T>(Func<Task<T>> action,CancellationToken ct)
    {
        // Keep draft validation and the write in one serializable transaction.
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        try { var result=await action();await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return result; }
        catch(DbUpdateException e) when(e.InnerException is PostgresException {SqlState:PostgresErrorCodes.UniqueViolation} pg)
        { throw new AccreditationConflictException(pg.ConstraintName=="aun_frameworks_code_version_uq"?"framework_version_exists":"duplicate_framework_item","Mã hoặc thứ tự đã tồn tại."); }
        catch(DbUpdateException e) when(e.InnerException is PostgresException {SqlState:PostgresErrorCodes.CheckViolation})
        { throw new AccreditationValidationException("Dữ liệu không hợp lệ.",new Dictionary<string,string[]>{{"framework",["Kiểm tra trạng thái, cấp đánh giá, thứ tự và số trang."]}}); }
    }
}
