using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfCoursePloMappingRepository(CurriculumDbContext db, CurriculumWriteSession writes) : ICoursePloMappingRepository
{
    private IQueryable<CoursePlo> Scope(long versionId, long courseId) => db.CoursePlos.Where(m => m.ProgramVersionId == versionId && m.ProgramCourseId == courseId);
    public async Task<IReadOnlyList<CoursePloMappingResponse>> GetCoursePloMappingsAsync(long versionId, long courseId, CancellationToken ct) =>
        await Scope(versionId, courseId).AsNoTracking().OrderBy(m => m.Plo.Code).ThenBy(m => m.Id).Select(CurriculumQueries.CoursePlo).ToListAsync(ct);
    public async Task<CoursePloMappingResponse> CreateCoursePloMappingAsync(long versionId, long courseId, NormalizedCoursePloMapping input, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            if (!await db.ProgramCourses.AnyAsync(c => c.Id == courseId && c.ProgramVersionId == versionId && c.ArchivedAt == null, ct)) throw new CurriculumNotFoundException("học phần trong phiên bản", courseId);
            if (!await db.Plos.AnyAsync(p => p.Id == input.PloId && p.ProgramVersionId == versionId, ct)) throw new CurriculumNotFoundException("PLO", input.PloId);
            var m = new CoursePlo { ProgramVersionId = versionId, ProgramCourseId = courseId, PloId = input.PloId, WeightCode = input.WeightCode[0],
                ProgressionCode = input.ProgressionCode[0], Fit = input.Fit, FitReason = input.FitReason, Provenance = "manual" };
            db.CoursePlos.Add(m); await db.SaveChangesAsync(ct); return m.Id;
        }, ct);
        return await Scope(versionId, courseId).AsNoTracking().Where(m => m.Id == id).Select(CurriculumQueries.CoursePlo).SingleAsync(ct);
    }
    public async Task<CoursePloMappingResponse?> UpdateCoursePloMappingAsync(long versionId, long courseId, long id, NormalizedCoursePloMappingUpdate input, CancellationToken ct)
    {
        var found = await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            var m = await Scope(versionId, courseId).SingleOrDefaultAsync(m => m.Id == id, ct);
            if (m == null) return false;
            m.WeightCode = input.WeightCode[0]; m.ProgressionCode = input.ProgressionCode[0]; m.Fit = input.Fit; m.FitReason = input.FitReason; m.Provenance = "manual"; return true;
        }, ct);
        return found ? await Scope(versionId, courseId).AsNoTracking().Where(m => m.Id == id).Select(CurriculumQueries.CoursePlo).SingleAsync(ct) : null;
    }
    public Task<bool> DeleteCoursePloMappingAsync(long versionId, long courseId, long id, CancellationToken ct) => writes.RunAsync(async () => {
        await writes.DraftAsync(versionId, ct);
        if (!await Scope(versionId, courseId).AnyAsync(m => m.Id == id, ct)) return false;
        if (await db.CloPlos.AnyAsync(m => m.CoursePloId == id, ct)) throw new CurriculumConflictException("course_plo_has_clo_mappings", "Không thể xóa mapping học phần-PLO đang được CLO sử dụng.");
        return await Scope(versionId, courseId).Where(m => m.Id == id).ExecuteDeleteAsync(ct) == 1;
    }, ct);
}
