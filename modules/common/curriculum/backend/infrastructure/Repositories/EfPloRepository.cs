using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfPloRepository(CurriculumDbContext db, CurriculumWriteSession writes) : IPloRepository
{
    public async Task<IReadOnlyList<PloResponse>> GetPlosAsync(long versionId, CancellationToken ct) =>
        await db.Plos.AsNoTracking().Where(p => p.ProgramVersionId == versionId).OrderBy(p => p.Code).ThenBy(p => p.Id).Select(CurriculumQueries.Plo).ToListAsync(ct);
    public Task<PloResponse?> GetPloAsync(long versionId, long id, CancellationToken ct) =>
        db.Plos.AsNoTracking().Where(p => p.ProgramVersionId == versionId && p.Id == id).Select(CurriculumQueries.Plo).SingleOrDefaultAsync(ct);
    public async Task<PloResponse> CreatePloAsync(long versionId, NormalizedLearningOutcome input, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            var p = new Plo { ProgramVersionId = versionId, Code = input.Code, Statement = input.Statement, LevelCode = input.LevelCode, Provenance = "manual" };
            db.Plos.Add(p); await db.SaveChangesAsync(ct); return p.Id;
        }, ct);
        return (await GetPloAsync(versionId, id, ct))!;
    }
    public async Task<PloResponse?> UpdatePloAsync(long versionId, long id, NormalizedLearningOutcome input, CancellationToken ct)
    {
        var found = await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            var p = await db.Plos.SingleOrDefaultAsync(p => p.Id == id && p.ProgramVersionId == versionId, ct);
            if (p == null) return false;
            p.Code = input.Code; p.Statement = input.Statement; p.LevelCode = input.LevelCode; p.Provenance = "manual"; return true;
        }, ct);
        return found ? await GetPloAsync(versionId, id, ct) : null;
    }
    public Task<bool> DeletePloAsync(long versionId, long id, CancellationToken ct) => writes.RunAsync(async () => {
        await writes.DraftAsync(versionId, ct);
        if (await db.CoursePlos.AnyAsync(m => m.PloId == id && m.ProgramVersionId == versionId, ct))
            throw new CurriculumConflictException("plo_has_course_mappings", "Không thể xóa PLO đang được ánh xạ với học phần.");
        return await db.Plos.Where(p => p.Id == id && p.ProgramVersionId == versionId).ExecuteDeleteAsync(ct) == 1;
    }, ct);
}
