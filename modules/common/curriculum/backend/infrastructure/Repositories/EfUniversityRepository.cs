using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfUniversityRepository(CurriculumDbContext db, CurriculumWriteSession writes) : IUniversityRepository
{
    public async Task<PagedResult<UniversityResponse>> GetUniversitiesAsync(int page, int pageSize, string? search, bool includeArchived, long[]? programIds, CancellationToken ct)
    {
        var query = db.Universities.AsNoTracking().Where(u => includeArchived || u.ArchivedAt == null);
        if (programIds != null) query = query.Where(u => u.Programs.Any(p => p.ArchivedAt == null && programIds.Contains(p.Id)));
        if (search != null) query = query.Where(u => EF.Functions.ILike(u.Code, "%" + search + "%") || EF.Functions.ILike(u.Name, "%" + search + "%"));
        var count = await query.LongCountAsync(ct);
        var items = await query.OrderBy(u => u.Name).ThenBy(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize).UniversityResponses(programIds).ToListAsync(ct);
        return PagedResult<UniversityResponse>.Create(items, page, pageSize, count);
    }

    public Task<UniversityResponse?> GetUniversityAsync(long id, CancellationToken ct) =>
        db.Universities.Where(u => u.Id == id).UniversityResponses().SingleOrDefaultAsync(ct);

    public async Task<UniversityResponse> CreateUniversityAsync(string code, string name, CancellationToken ct)
    {
        var entity = new University { Code = code, Name = name };
        await writes.RunAsync(async () => { db.Universities.Add(entity); await db.SaveChangesAsync(ct); return entity.Id; }, ct);
        return (await GetUniversityAsync(entity.Id, ct))!;
    }

    public async Task<UniversityResponse?> UpdateUniversityAsync(long id, string name, CancellationToken ct)
    {
        var entity = await db.Universities.SingleOrDefaultAsync(u => u.Id == id && u.ArchivedAt == null, ct);
        if (entity == null) return null;
        entity.Name = name;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetUniversityAsync(id, ct);
    }

    public Task<bool> ArchiveUniversityAsync(long id, CancellationToken ct) => writes.RunAsync(async () =>
    {
        if (await db.Programs.AnyAsync(p => p.UniversityId == id && p.ArchivedAt == null, ct))
            throw new CurriculumConflictException("university_has_programs", "Chỉ lưu trữ được trường khi không còn CTĐT đang hoạt động.");
        return await db.Universities.Where(u => u.Id == id && u.ArchivedAt == null).ExecuteUpdateAsync(s => s
            .SetProperty(u => u.ArchivedAt, DateTime.UtcNow).SetProperty(u => u.UpdatedAt, DateTime.UtcNow), ct) == 1;
    }, ct);

    public async Task<IReadOnlyDictionary<long, long[]>> GetProgramIdsAsync(long[] universityIds, CancellationToken ct)
    {
        var rows = await db.Programs.AsNoTracking()
            .Where(p => p.ArchivedAt == null && universityIds.Contains(p.UniversityId))
            .Select(p => new { p.UniversityId, p.Id })
            .ToListAsync(ct);
        return universityIds.Distinct().ToDictionary(id => id, id => rows.Where(r => r.UniversityId == id).Select(r => r.Id).ToArray());
    }
}
