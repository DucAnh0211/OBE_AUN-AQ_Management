using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal static class CurriculumQueries
{
    public static IQueryable<ProgramResponse> ProgramResponses(this IQueryable<Domain.Entities.Program> query) =>
        query.AsNoTracking().Select(p => new ProgramResponse(p.Id, p.Code, p.Name, p.ArchivedAt != null,
            p.ProgramVersions.LongCount(v => v.ArchivedAt == null),
            p.ProgramVersions.Where(v => v.IsCurrent && v.ArchivedAt == null).Select(v => (long?)v.Id).FirstOrDefault(),
            p.ProgramVersions.Where(v => v.IsCurrent && v.ArchivedAt == null).Select(v => v.VersionCode).FirstOrDefault(), p.CreatedAt, p.UpdatedAt));
    public static IQueryable<ProgramVersionResponse> VersionResponses(this IQueryable<ProgramVersion> query) =>
        query.AsNoTracking().Select(v => new ProgramVersionResponse(v.Id, v.ProgramId, v.VersionCode,
            v.DatasetStatus, v.IsCurrent, v.SourceVersionId, v.ProgramCourses.LongCount(c => c.ArchivedAt == null),
            v.PublishedAt, v.ArchivedAt, v.CreatedAt, v.UpdatedAt));
    public static readonly Expression<Func<ProgramCourse, ProgramCourseResponse>> Course = c => new(
        c.Id, c.ProgramVersionId, c.CourseId, c.CourseCodeSnapshot, c.CourseNameSnapshot, c.Credits,
        c.SemesterSourceValue, c.DisplayOrder, c.DatasetStatus, c.SourceRow, c.ArchivedAt, c.CreatedAt, c.UpdatedAt);
    public static readonly Expression<Func<Plo, PloResponse>> Plo = p => new(p.Id, p.ProgramVersionId, p.Code, p.Statement, p.LevelCode, p.Provenance);
    public static readonly Expression<Func<Clo, CloResponse>> Clo = c => new(c.Id, c.ProgramCourseId, c.Code, c.Statement, c.LevelCode, c.Provenance, c.DatasetStatus);
    public static readonly Expression<Func<CoursePlo, CoursePloMappingResponse>> CoursePlo = m => new(m.Id, m.ProgramVersionId,
        m.ProgramCourseId, m.PloId, m.Plo.Code, m.WeightCode.ToString(), m.ProgressionCode.ToString(), m.Provenance, m.Fit, m.FitReason);
    public static readonly Expression<Func<CloPlo, CloPloMappingResponse>> CloPlo = m => new(m.CloId, m.Clo.Code, m.CoursePloId, m.CoursePlo.PloId, m.CoursePlo.Plo.Code);
}
