using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfCurriculumAnalysisRepository(CurriculumDbContext db) : ICurriculumAnalysisRepository
{
    public async Task<IReadOnlyList<CoursePloCreditCheckResponse>> GetPloCreditChecksAsync(long versionId, CancellationToken ct)
    {
        var rows = await db.ProgramCourses.AsNoTracking().Where(c => c.ProgramVersionId == versionId && c.ArchivedAt == null)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).Select(c => new { c.Id, c.CourseCodeSnapshot, c.CourseNameSnapshot, c.Credits, Count = c.CoursePlos.Count }).ToListAsync(ct);
        return rows.Select(c => new CoursePloCreditCheckResponse(c.Id, c.CourseCodeSnapshot, c.CourseNameSnapshot, c.Credits,
            CurriculumAnalysisRules.RequiredPlos(c.Credits), c.Count, c.Count >= CurriculumAnalysisRules.RequiredPlos(c.Credits))).ToArray();
    }
    public async Task<IReadOnlyList<PloBalanceItemResponse>> GetPloBalanceAsync(long versionId, CancellationToken ct)
    {
        var rows = await db.Plos.AsNoTracking().Where(p => p.ProgramVersionId == versionId).OrderBy(p => p.Code).ThenBy(p => p.Id)
            .Select(p => new { p.Code, Score = p.CoursePlos.Where(m => m.ProgramCourse.ArchivedAt == null)
                .Sum(m => (decimal?)m.ProgramCourse.Credits * m.WeightCodeNavigation.Score) ?? 0m }).ToListAsync(ct);
        var mean = rows.Count == 0 ? 0 : rows.Average(p => p.Score);
        return rows.Select(p => {
            var deviation = CurriculumAnalysisRules.Deviation(p.Score, mean);
            return new PloBalanceItemResponse(p.Code, p.Score, mean, deviation == null ? null : Math.Round(deviation.Value, 4, MidpointRounding.AwayFromZero), CurriculumAnalysisRules.ExceedsLimit(deviation));
        }).ToArray();
    }
}
