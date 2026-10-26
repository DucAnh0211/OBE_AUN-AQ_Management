using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfCloRepository(CurriculumDbContext db, CurriculumWriteSession writes) : ICloRepository
{
    private IQueryable<Clo> Scope(long versionId, long courseId) => db.Clos.Where(c => c.ProgramCourseId == courseId && c.ProgramCourse.ProgramVersionId == versionId);
    public async Task<IReadOnlyList<CloResponse>> GetClosAsync(long versionId, long courseId, CancellationToken ct) =>
        await Scope(versionId, courseId).AsNoTracking().OrderBy(c => c.Code).ThenBy(c => c.Id).Select(CurriculumQueries.Clo).ToListAsync(ct);
    public Task<CloResponse?> GetCloAsync(long versionId, long courseId, long id, CancellationToken ct) =>
        Scope(versionId, courseId).AsNoTracking().Where(c => c.Id == id).Select(CurriculumQueries.Clo).SingleOrDefaultAsync(ct);
    public async Task<CloResponse> CreateCloAsync(long versionId, long courseId, NormalizedLearningOutcome input, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            if (!await db.ProgramCourses.AnyAsync(c => c.Id == courseId && c.ProgramVersionId == versionId && c.ArchivedAt == null, ct))
                throw new CurriculumNotFoundException("học phần trong phiên bản", courseId);
            var c = new Clo { ProgramCourseId = courseId, Code = input.Code, Statement = input.Statement, LevelCode = input.LevelCode, Provenance = "manual", DatasetStatus = "draft" };
            db.Clos.Add(c); await db.SaveChangesAsync(ct); return c.Id;
        }, ct);
        return (await GetCloAsync(versionId, courseId, id, ct))!;
    }
    public async Task<CloResponse?> UpdateCloAsync(long versionId, long courseId, long id, NormalizedLearningOutcome input, CancellationToken ct)
    {
        var found = await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            var c = await Scope(versionId, courseId).SingleOrDefaultAsync(c => c.Id == id && c.ProgramCourse.ArchivedAt == null, ct);
            if (c == null) return false;
            c.Code = input.Code; c.Statement = input.Statement; c.LevelCode = input.LevelCode; c.Provenance = "manual"; c.DatasetStatus = "draft"; return true;
        }, ct);
        return found ? await GetCloAsync(versionId, courseId, id, ct) : null;
    }
    public Task<bool> DeleteCloAsync(long versionId, long courseId, long id, CancellationToken ct) => writes.RunAsync(async () => {
        await writes.DraftAsync(versionId, ct);
        if (!await Scope(versionId, courseId).AnyAsync(c => c.Id == id && c.ProgramCourse.ArchivedAt == null, ct)) return false;
        await db.CloPlos.Where(m => m.CloId == id).ExecuteDeleteAsync(ct);
        return await Scope(versionId, courseId).Where(c => c.Id == id).ExecuteDeleteAsync(ct) == 1;
    }, ct);
}
