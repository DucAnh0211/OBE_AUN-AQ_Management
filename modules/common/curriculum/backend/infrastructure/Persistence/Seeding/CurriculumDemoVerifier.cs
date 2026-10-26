using Microsoft.EntityFrameworkCore;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

// Verifies the packaged demo, not a general validation rule for user-edited curricula.
public sealed class CurriculumDemoVerifier(CurriculumDbContext db)
{
    public async Task VerifyAsync(CancellationToken ct = default)
    {
        foreach (var code in new[] { "V1", "V2" })
        {
            var v = await db.ProgramVersions.AsNoTracking().SingleAsync(v => v.Program.Code == "CS" && v.VersionCode == code, ct);
            var courses = db.ProgramCourses.Where(c => c.ProgramVersionId == v.Id);
            var plos = db.Plos.Where(p => p.ProgramVersionId == v.Id);
            var mappings = db.CoursePlos.Where(m => m.ProgramVersionId == v.Id);
            var clos = db.Clos.Where(c => c.ProgramCourse.ProgramVersionId == v.Id);
            var links = db.CloPlos.Where(m => m.CoursePlo.ProgramVersionId == v.Id);
            Check(await courses.CountAsync(ct) == 59 && await plos.CountAsync(ct) == 5, code + " course/PLO counts");
            Check(await mappings.CountAsync(ct) == (code == "V1" ? 146 : 151), code + " course-PLO count");
            Check(await clos.CountAsync(ct) == (code == "V1" ? 146 : 155), code + " CLO count");
            Check(await links.CountAsync(ct) == (code == "V1" ? 146 : 210), code + " CLO-PLO count");
            Check(await db.SourceCourseRows.CountAsync(r => r.ImportBatch.ProgramVersionId == v.Id, ct) == 59, code + " source rows");
            Check(!await clos.AnyAsync(c => !c.CloPlos.Any(), ct), code + " CLO without PLO");
            Check(!await mappings.AnyAsync(m => !m.CloPlos.Any(), ct), code + " PLO without supporting CLO");
            if (code == "V2")
            {
                Check(!await courses.AnyAsync(c => c.CoursePlos.Count < Math.Min(c.Credits, 5), ct), "V2 credit rule");
                Check(!await courses.AnyAsync(c => c.Clos.Count < 2 || c.Clos.Count > 5, ct), "V2 CLO range");
                Check(await clos.CountAsync(c => c.CloPlos.Count > 1, ct) == 55, "V2 many-to-many mappings");
                var scores = await plos.Select(p => p.CoursePlos.Sum(m => (decimal)m.ProgramCourse.Credits * m.WeightCodeNavigation.Score)).ToListAsync(ct);
                var mean = scores.Average();
                Check(mean > 0 && scores.All(score => Math.Abs(score - mean) / mean <= .2m), "V2 PLO balance");
            }
        }
    }

    private static void Check(bool condition, string rule)
    {
        if (!condition) throw new InvalidOperationException("Demo verification failed: " + rule);
    }
}
