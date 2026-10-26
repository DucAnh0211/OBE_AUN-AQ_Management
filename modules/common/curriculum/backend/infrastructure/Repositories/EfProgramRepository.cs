using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ProgramEntity = ObeAunQa.Modules.Curriculum.Domain.Entities.Program;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfProgramRepository(CurriculumDbContext db, CurriculumWriteSession writes) : IProgramRepository
{
    public async Task<PagedResult<ProgramResponse>> GetProgramsAsync(int page, int pageSize, string? search, bool includeArchived, CancellationToken ct)
    {
        var query = db.Programs.AsNoTracking().Where(p => includeArchived || p.ArchivedAt == null);
        if (search != null) query = query.Where(p => EF.Functions.ILike(p.Code, "%" + search + "%") || EF.Functions.ILike(p.Name, "%" + search + "%"));
        var count = await query.LongCountAsync(ct);
        return PagedResult<ProgramResponse>.Create(await query.OrderBy(p => p.Code).ThenBy(p => p.Id).Skip((page-1)*pageSize).Take(pageSize).ProgramResponses().ToListAsync(ct), page, pageSize, count);
    }
    public Task<ProgramResponse?> GetProgramAsync(long id, CancellationToken ct) => db.Programs.Where(p => p.Id == id).ProgramResponses().SingleOrDefaultAsync(ct);
    public async Task<ProgramResponse> CreateProgramAsync(string code, string name, CancellationToken ct)
    {
        var entity = new ProgramEntity { Code = code, Name = name };
        await writes.RunAsync(async () => { db.Programs.Add(entity); await db.SaveChangesAsync(ct); return entity.Id; }, ct);
        return (await GetProgramAsync(entity.Id, ct))!;
    }
    public async Task<ProgramResponse?> UpdateProgramAsync(long id, string name, CancellationToken ct)
    {
        var entity = await db.Programs.SingleOrDefaultAsync(p => p.Id == id && p.ArchivedAt == null, ct);
        if (entity == null) return null;
        entity.Name = name; entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetProgramAsync(id, ct);
    }
    public async Task<bool> ArchiveProgramAsync(long id, CancellationToken ct) =>
        await db.Programs.Where(p => p.Id == id && p.ArchivedAt == null).ExecuteUpdateAsync(s => s
            .SetProperty(p => p.ArchivedAt, DateTime.UtcNow).SetProperty(p => p.UpdatedAt, DateTime.UtcNow), ct) == 1;
}
