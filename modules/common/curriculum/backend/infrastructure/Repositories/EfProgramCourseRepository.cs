using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfProgramCourseRepository(CurriculumDbContext db, CurriculumWriteSession writes) : IProgramCourseRepository
{
    public async Task<PagedResult<ProgramCourseResponse>> GetProgramCoursesAsync(long versionId, int page, int pageSize, string? search, string? semester, bool includeArchived, CancellationToken ct)
    {
        var query = db.ProgramCourses.AsNoTracking().Where(c => c.ProgramVersionId == versionId && (includeArchived || c.ArchivedAt == null));
        if (search != null) query = query.Where(c => EF.Functions.ILike(c.CourseNameSnapshot, "%"+search+"%") || (c.CourseCodeSnapshot != null && EF.Functions.ILike(c.CourseCodeSnapshot, "%"+search+"%")));
        if (semester != null) query = query.Where(c => c.SemesterSourceValue != null && EF.Functions.ILike(c.SemesterSourceValue, semester));
        var count = await query.LongCountAsync(ct);
        return PagedResult<ProgramCourseResponse>.Create(await query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).Skip((page-1)*pageSize).Take(pageSize).Select(CurriculumQueries.Course).ToListAsync(ct), page, pageSize, count);
    }
    public Task<ProgramCourseResponse?> GetProgramCourseAsync(long versionId, long id, CancellationToken ct) => db.ProgramCourses.AsNoTracking().Where(c => c.Id == id && c.ProgramVersionId == versionId).Select(CurriculumQueries.Course).SingleOrDefaultAsync(ct);
    public async Task<ProgramCourseResponse> CreateProgramCourseAsync(long versionId, NormalizedCourse input, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            var entity = new ProgramCourse { ProgramVersionId = versionId, CourseId = await CourseId(input, ct), DatasetStatus = CurriculumStatuses.Draft, QualityFlags = "[]",
                CourseCodeSnapshot = input.InstitutionalCode, CourseNameSnapshot = input.Name, Credits = input.Credits, SemesterSourceValue = input.Semester,
                DisplayOrder = input.DisplayOrder ?? (await db.ProgramCourses.Where(c => c.ProgramVersionId == versionId).MaxAsync(c => (int?)c.DisplayOrder, ct) ?? 0) + 1 };
            db.ProgramCourses.Add(entity); await db.SaveChangesAsync(ct); return entity.Id;
        }, ct);
        return (await GetProgramCourseAsync(versionId, id, ct))!;
    }
    public async Task<ProgramCourseResponse?> UpdateProgramCourseAsync(long versionId, long id, NormalizedCourse input, CancellationToken ct)
    {
        var found = await writes.RunAsync(async () => {
            await writes.DraftAsync(versionId, ct);
            var c = await db.ProgramCourses.SingleOrDefaultAsync(c => c.Id == id && c.ProgramVersionId == versionId && c.ArchivedAt == null, ct);
            if (c == null) return false;
            if (input.InstitutionalCode != null) c.CourseId = await CourseId(input, ct);
            c.CourseCodeSnapshot = input.InstitutionalCode; c.CourseNameSnapshot = input.Name; c.Credits = input.Credits;
            c.SemesterSourceValue = input.Semester; c.DisplayOrder = input.DisplayOrder!.Value; c.UpdatedAt = DateTime.UtcNow;
            return true;
        }, ct);
        return found ? await GetProgramCourseAsync(versionId, id, ct) : null;
    }
    public Task<bool> ArchiveProgramCourseAsync(long versionId, long id, CancellationToken ct) => writes.RunAsync(async () => {
        await writes.DraftAsync(versionId, ct);
        return await db.ProgramCourses.Where(c => c.Id == id && c.ProgramVersionId == versionId && c.ArchivedAt == null).ExecuteUpdateAsync(s => s
            .SetProperty(c => c.DatasetStatus, CurriculumStatuses.Archived).SetProperty(c => c.ArchivedAt, DateTime.UtcNow).SetProperty(c => c.UpdatedAt, DateTime.UtcNow), ct) == 1;
    }, ct);
    private async Task<long> CourseId(NormalizedCourse input, CancellationToken ct)
    {
        if (input.InstitutionalCode != null)
        {
            var existing = await db.Courses.SingleOrDefaultAsync(c => c.InstitutionalCode == input.InstitutionalCode, ct);
            if (existing != null) return existing.Id;
        }
        var course = new Course { InstitutionalCode = input.InstitutionalCode, Name = input.Name };
        db.Courses.Add(course); await db.SaveChangesAsync(ct); return course.Id;
    }
}
