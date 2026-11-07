using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfCourseOfferingRepository(CurriculumDbContext db, CurriculumWriteSession writes) : ICourseOfferingRepository
{
    private IQueryable<AcademicClass> Classes(long version, long? lecturer) => db.Set<AcademicClass>().AsNoTracking()
        .Where(c => c.ProgramVersionId == version && (lecturer == null || db.Set<CourseOffering>().Any(o => o.AcademicClassId == c.Id && o.LecturerId == lecturer)));
    private IQueryable<CourseOffering> Offerings(long version) => db.Set<CourseOffering>().AsNoTracking().Where(o => o.ProgramVersionId == version);
    private IQueryable<CourseOfferingInfo> OfferingInfos(IQueryable<CourseOffering> query) => query.Select(o => new CourseOfferingInfo(o.Id, o.AcademicClassId, o.ProgramVersionId, o.ProgramCourseId, o.AcademicPeriod,
            o.Code, o.AcademicClass.Code, o.AcademicClass.Name, o.ProgramCourse.CourseCodeSnapshot, o.ProgramCourse.CourseNameSnapshot,
            o.ProgramCourse.Credits, o.LecturerId, null, o.ProgramCourse.ArchivedAt != null || o.ProgramCourse.ProgramVersion.ArchivedAt != null,
            db.Set<CourseEnrollment>().Count(e => e.OfferingId == o.Id)));
    private IQueryable<AcademicClassInfo> ClassInfos(IQueryable<AcademicClass> query, long? lecturer) => query.Select(c => new AcademicClassInfo(
        c.Id, c.ProgramVersionId, c.AcademicPeriod, c.Code, c.Name, db.Set<CourseOffering>().Count(o => o.AcademicClassId == c.Id && (lecturer == null || o.LecturerId == lecturer))));
    public async Task<IReadOnlyList<string>> PeriodsAsync(long versionId, long? lecturerId, CancellationToken ct) =>
        await Classes(versionId, lecturerId).Select(c => c.AcademicPeriod).Distinct().ToArrayAsync(ct);
    public async Task<PagedResult<AcademicClassInfo>> ClassesAsync(long versionId, string period, int page, string? search, long? lecturerId, CancellationToken ct)
    {
        var query = Classes(versionId, lecturerId).Where(c => c.AcademicPeriod == period);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => EF.Functions.ILike(c.Code, "%" + search + "%") || EF.Functions.ILike(c.Name, "%" + search + "%"));
        var total = await query.LongCountAsync(ct);
        var rows = await ClassInfos(query.OrderBy(c => c.Code).Skip((page - 1) * 50).Take(50), lecturerId).ToArrayAsync(ct);
        return PagedResult<AcademicClassInfo>.Create(rows, page, 50, total);
    }
    public async Task<AcademicClassInfo> ClassAsync(long versionId, long classId, long? lecturerId, CancellationToken ct) =>
        await ClassInfos(Classes(versionId, lecturerId).Where(c => c.Id == classId), lecturerId).SingleOrDefaultAsync(ct)
        ?? throw new CurriculumNotFoundException("lớp", classId);
    public async Task<AcademicClassInfo> CreateClassAsync(long versionId, CreateAcademicClass request, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            await writes.LockVersionAsync(versionId, ct);
            var row = new AcademicClass { ProgramVersionId = versionId, AcademicPeriod = request.AcademicPeriod, Code = request.Code, Name = request.Name, CreatedAt = DateTime.UtcNow };
            db.Add(row); await db.SaveChangesAsync(ct); return row.Id;
        }, ct);
        return await ClassAsync(versionId, id, null, ct);
    }
    public async Task<IReadOnlyList<CourseOfferingInfo>> OfferingsAsync(long versionId, long classId, long? lecturerId, CancellationToken ct) =>
        await OfferingInfos(Offerings(versionId).Where(o => o.AcademicClassId == classId && (lecturerId == null || o.LecturerId == lecturerId)).OrderBy(o => o.Code)).ToArrayAsync(ct);
    public async Task<PagedResult<CourseOfferingInfo>> SubjectOfferingsAsync(long versionId, long courseId, int page, string? search, long? lecturerId, CancellationToken ct)
    {
        var query = Offerings(versionId).Where(o => o.ProgramCourseId == courseId && (lecturerId == null || o.LecturerId == lecturerId));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(o => EF.Functions.ILike(o.Code, "%" + search + "%")
            || EF.Functions.ILike(o.AcademicClass.Code, "%" + search + "%") || EF.Functions.ILike(o.AcademicClass.Name, "%" + search + "%")
            || EF.Functions.ILike(o.AcademicPeriod, "%" + search + "%"));
        var count = await query.LongCountAsync(ct);
        var rows = await OfferingInfos(query.OrderByDescending(o => o.AcademicPeriod).ThenBy(o => o.Code).Skip((page - 1) * 50).Take(50)).ToArrayAsync(ct);
        return PagedResult<CourseOfferingInfo>.Create(rows, page, 50, count);
    }
    public async Task<CourseOfferingInfo> OfferingAsync(long versionId, long offeringId, CancellationToken ct) =>
        await OfferingInfos(Offerings(versionId).Where(o => o.Id == offeringId)).SingleOrDefaultAsync(ct) ?? throw new CurriculumNotFoundException("lớp học phần", offeringId);
    public async Task<CourseOfferingInfo> CreateOfferingAsync(long versionId, long classId, CreateCourseOffering request, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            await writes.LockVersionAsync(versionId, ct);
            var cls = await ClassAsync(versionId, classId, null, ct);
            if (!await db.ProgramCourses.AnyAsync(c => c.Id == request.ProgramCourseId && c.ProgramVersionId == versionId && c.ArchivedAt == null, ct))
                throw new CurriculumNotFoundException("môn gốc đang hoạt động", request.ProgramCourseId);
            if (await db.Set<CourseOffering>().AnyAsync(o => o.AcademicClassId == classId && o.ProgramCourseId == request.ProgramCourseId, ct))
                throw new CurriculumConflictException("subject_already_open", "Môn này đã được mở trong lớp. Hãy chọn học phần đã có.");
            var row = new CourseOffering { AcademicClassId = classId, ProgramVersionId = versionId, ProgramCourseId = request.ProgramCourseId,
                AcademicPeriod = cls.AcademicPeriod, LecturerId = request.LecturerId, CreatedAt = DateTime.UtcNow };
            db.Add(row); await db.SaveChangesAsync(ct); return row.Id;
        }, ct);
        return await OfferingAsync(versionId, id, ct);
    }
    public async Task<CourseOfferingInfo> AssignLecturerAsync(long versionId, long offeringId, long? lecturerId, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            await writes.LockVersionAsync(versionId, ct);
            var row = await db.Set<CourseOffering>().SingleOrDefaultAsync(o => o.Id == offeringId && o.ProgramVersionId == versionId, ct)
                ?? throw new CurriculumNotFoundException("lớp học phần", offeringId);
            row.LecturerId = lecturerId; return true;
        }, ct);
        return await OfferingAsync(versionId, offeringId, ct);
    }
    public async Task<long> EnrollmentOfferingAsync(long courseId, long enrollmentId, CancellationToken ct) =>
        await db.Set<CourseEnrollment>().Where(e => e.Id == enrollmentId && e.ProgramCourseId == courseId).Select(e => (long?)e.OfferingId).SingleOrDefaultAsync(ct)
        ?? throw new CurriculumNotFoundException("sinh viên trong học phần", enrollmentId);
}
