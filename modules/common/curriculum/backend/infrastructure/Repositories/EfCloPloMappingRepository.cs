using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfCloPloMappingRepository(CurriculumDbContext db, CurriculumWriteSession writes) : ICloPloMappingRepository
{
    private IQueryable<CloPlo> Scope(long versionId, long courseId, long cloId) => db.CloPlos.Where(m => m.CoursePlo.ProgramVersionId == versionId && m.ProgramCourseId == courseId && m.CloId == cloId);
    public async Task<IReadOnlyList<CloPloMappingResponse>> GetCloPloMappingsAsync(long versionId, long courseId, long cloId, CancellationToken ct) =>
        await Scope(versionId, courseId, cloId).AsNoTracking().OrderBy(m => m.CoursePlo.Plo.Code).ThenBy(m => m.CoursePloId).Select(CurriculumQueries.CloPlo).ToListAsync(ct);
    public async Task<CloPloMappingResponse> CreateCloPloMappingAsync(long versionId, long courseId, long cloId, long mappingId, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            if (!await db.Clos.AnyAsync(c => c.Id == cloId && c.ProgramCourseId == courseId && c.ProgramCourse.ProgramVersionId == versionId && c.ProgramCourse.ArchivedAt == null, ct)) throw new CurriculumNotFoundException("CLO", cloId);
            if (!await db.CoursePlos.AnyAsync(m => m.Id == mappingId && m.ProgramCourseId == courseId && m.ProgramVersionId == versionId, ct)) throw new CurriculumNotFoundException("mapping học phần-PLO", mappingId);
            db.CloPlos.Add(new CloPlo { CloId = cloId, ProgramCourseId = courseId, CoursePloId = mappingId }); return true;
        }, ct);
        return await Scope(versionId, courseId, cloId).AsNoTracking().Where(m => m.CoursePloId == mappingId).Select(CurriculumQueries.CloPlo).SingleAsync(ct);
    }
    public Task<bool> DeleteCloPloMappingAsync(long versionId, long courseId, long cloId, long mappingId, CancellationToken ct) => writes.RunAsync(async () => {
        await writes.DraftAsync(versionId, ct);
        return await Scope(versionId, courseId, cloId).Where(m => m.CoursePloId == mappingId).ExecuteDeleteAsync(ct) == 1;
    }, ct);
}
