using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal static class GradingQueries
{
    /// <summary>Quy chế và thang điểm chữ của trường; trường chưa cấu hình dùng mặc định Thông tư 08/2021.</summary>
    public static async Task<(GradingPolicy Policy, GradeBand[] Bands)> RulesAsync(this CurriculumDbContext db, long universityId, CancellationToken ct)
    {
        var policy = await db.Set<GradingPolicy>().AsNoTracking().SingleOrDefaultAsync(p => p.UniversityId == universityId, ct) ?? new GradingPolicy { UniversityId = universityId };
        var bands = await db.Set<GradeScaleBand>().AsNoTracking().Where(b => b.UniversityId == universityId).OrderByDescending(b => b.MinScore)
            .Select(b => new GradeBand(b.Letter, b.MinScore, b.Point4, b.IsPassing)).ToArrayAsync(ct);
        return (policy, bands.Length > 0 ? bands : GradeRules.DefaultBands.ToArray());
    }
    public static Task<long> UniversityOfVersionAsync(this CurriculumDbContext db, long versionId, CancellationToken ct) =>
        db.ProgramVersions.Where(v => v.Id == versionId).Select(v => v.Program.UniversityId).SingleAsync(ct);
    public static GradingPolicyInfo Info(GradingPolicy p, IEnumerable<GradeBand> bands) => new(p.UniversityId, p.CourseScoreDecimals, p.MinComponents,
        p.MinFinalWeight, p.RetakePolicy, p.GpaDecimals, bands.OrderByDescending(b => b.MinScore).Select(b => new GradeBandInput(b.Letter, b.MinScore, b.Point4, b.IsPassing)).ToArray());
}
