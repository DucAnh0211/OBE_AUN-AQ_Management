using System.Data;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfCurriculumReportSource(CurriculumDbContext db) : ICurriculumReportSource
{
    public async Task<CurriculumReportSnapshot?> ReadAsync(long versionId, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var version = await db.ProgramVersions.Where(v => v.Id == versionId && v.ArchivedAt == null).VersionResponses().SingleOrDefaultAsync(ct);
        if (version == null) return null;
        var program = await db.Programs.Where(p => p.Id == version.ProgramId && p.ArchivedAt == null).ProgramResponses().SingleOrDefaultAsync(ct);
        if (program == null) return null;
        var courses = await db.ProgramCourses.AsNoTracking().Where(c => c.ProgramVersionId == versionId && c.ArchivedAt == null).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).Select(CurriculumQueries.Course).ToListAsync(ct);
        var plos = await db.Plos.AsNoTracking().Where(p => p.ProgramVersionId == versionId).OrderBy(p => p.Code).ThenBy(p => p.Id).Select(CurriculumQueries.Plo).ToListAsync(ct);
        var clos = await db.Clos.AsNoTracking().Where(c => c.ProgramCourse.ProgramVersionId == versionId && c.ProgramCourse.ArchivedAt == null && c.DatasetStatus != "archived")
            .OrderBy(c => c.ProgramCourse.DisplayOrder).ThenBy(c => c.Code).ThenBy(c => c.Id).Select(CurriculumQueries.Clo).ToListAsync(ct);
        var courseMappings = await db.CoursePlos.AsNoTracking().Where(m => m.ProgramVersionId == versionId && m.ProgramCourse.ArchivedAt == null)
            .OrderBy(m => m.ProgramCourse.DisplayOrder).ThenBy(m => m.Plo.Code).ThenBy(m => m.Id).Select(CurriculumQueries.CoursePlo).ToListAsync(ct);
        var cloMappings = await db.CloPlos.AsNoTracking().Where(m => m.CoursePlo.ProgramVersionId == versionId && m.Clo.ProgramCourse.ArchivedAt == null && m.Clo.DatasetStatus != "archived")
            .OrderBy(m => m.CloId).ThenBy(m => m.CoursePloId).Select(CurriculumQueries.CloPlo).ToListAsync(ct);
        await transaction.CommitAsync(ct);
        return new(program, version, courses, plos, clos, courseMappings, cloMappings);
    }
}
