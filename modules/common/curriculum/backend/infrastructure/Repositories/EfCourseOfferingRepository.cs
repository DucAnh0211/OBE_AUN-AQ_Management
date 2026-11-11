using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfCourseOfferingRepository(CurriculumDbContext db, CurriculumWriteSession writes) : ICourseOfferingRepository
{
    private IQueryable<CourseOffering> Offerings(long version, long? lecturer) => db.Set<CourseOffering>().AsNoTracking()
        .Where(o => o.ProgramVersionId == version && (lecturer == null || o.Lecturers.Any(l => l.UserId == lecturer)));
    private IQueryable<CourseOfferingInfo> OfferingInfos(IQueryable<CourseOffering> query) => query.Select(o => new CourseOfferingInfo(o.Id, o.ProgramVersionId,
        o.ProgramCourseId, o.AcademicPeriod, o.Code, o.GroupName, o.ProgramCourse.CourseCodeSnapshot, o.ProgramCourse.CourseNameSnapshot, o.ProgramCourse.Credits,
        o.Lecturers.Select(l => new OfferingLecturerInfo(l.UserId, null, null, l.Role)).ToList(),
        o.ProgramCourse.ArchivedAt != null || o.ProgramCourse.ProgramVersion.ArchivedAt != null,
        db.Set<CourseEnrollment>().Count(e => e.OfferingId == o.Id), o.Capacity, o.TargetHomeClassId,
        db.Set<HomeClass>().Where(h => h.Id == o.TargetHomeClassId).Select(h => h.Code).FirstOrDefault(),
        o.GradeStatus, o.StatusNote, o.PublishedAt));
    public async Task<IReadOnlyList<string>> PeriodsAsync(long versionId, long? lecturerId, CancellationToken ct) =>
        await Offerings(versionId, lecturerId).Select(o => o.AcademicPeriod).Distinct().ToArrayAsync(ct);
    public async Task<PagedResult<CourseOfferingInfo>> OfferingsAsync(long versionId, string period, int page, string? search, long? lecturerId, CancellationToken ct)
    {
        var query = Offerings(versionId, lecturerId).Where(o => o.AcademicPeriod == period);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(o => EF.Functions.ILike(o.Code, "%" + search + "%") || EF.Functions.ILike(o.GroupName, "%" + search + "%")
            || EF.Functions.ILike(o.ProgramCourse.CourseCodeSnapshot, "%" + search + "%") || EF.Functions.ILike(o.ProgramCourse.CourseNameSnapshot, "%" + search + "%"));
        var total = await query.LongCountAsync(ct);
        var rows = await OfferingInfos(query.OrderBy(o => o.ProgramCourse.CourseCodeSnapshot).ThenBy(o => o.GroupName).Skip((page - 1) * 50).Take(50)).ToArrayAsync(ct);
        return PagedResult<CourseOfferingInfo>.Create(rows, page, 50, total);
    }
    public async Task<PagedResult<CourseOfferingInfo>> SubjectOfferingsAsync(long versionId, long courseId, int page, string? search, long? lecturerId, CancellationToken ct)
    {
        var query = Offerings(versionId, lecturerId).Where(o => o.ProgramCourseId == courseId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(o => EF.Functions.ILike(o.Code, "%" + search + "%")
            || EF.Functions.ILike(o.GroupName, "%" + search + "%") || EF.Functions.ILike(o.AcademicPeriod, "%" + search + "%"));
        var count = await query.LongCountAsync(ct);
        var rows = await OfferingInfos(query.OrderByDescending(o => o.AcademicPeriod).ThenBy(o => o.GroupName).Skip((page - 1) * 50).Take(50)).ToArrayAsync(ct);
        return PagedResult<CourseOfferingInfo>.Create(rows, page, 50, count);
    }
    public async Task<CourseOfferingInfo> OfferingAsync(long versionId, long offeringId, CancellationToken ct) =>
        await OfferingInfos(Offerings(versionId, null).Where(o => o.Id == offeringId)).SingleOrDefaultAsync(ct) ?? throw new CurriculumNotFoundException("lớp học phần", offeringId);
    private async Task TargetClass(long versionId, long? homeClassId, CancellationToken ct)
    {
        if (homeClassId is long id && !await db.Set<HomeClass>().AnyAsync(h => h.Id == id && h.ProgramVersionId == versionId, ct))
            throw new CurriculumNotFoundException("lớp sinh hoạt trong phiên bản CTĐT", id);
    }
    private async Task<CourseOffering> Lock(long versionId, long offeringId, CancellationToken ct)
    {
        await writes.LockVersionAsync(versionId, ct);
        return await db.Set<CourseOffering>().Include(o => o.Lecturers).SingleOrDefaultAsync(o => o.Id == offeringId && o.ProgramVersionId == versionId, ct)
            ?? throw new CurriculumNotFoundException("lớp học phần", offeringId);
    }
    public async Task<CourseOfferingInfo> CreateOfferingAsync(long versionId, CreateCourseOffering request, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            await writes.LockVersionAsync(versionId, ct);
            if (!await db.ProgramCourses.AnyAsync(c => c.Id == request.ProgramCourseId && c.ProgramVersionId == versionId && c.ArchivedAt == null, ct))
                throw new CurriculumNotFoundException("học phần đang hoạt động", request.ProgramCourseId);
            await TargetClass(versionId, request.TargetHomeClassId, ct);
            var used = (await db.Set<CourseOffering>().Where(o => o.ProgramCourseId == request.ProgramCourseId && o.AcademicPeriod == request.AcademicPeriod)
                .Select(o => o.GroupName).ToArrayAsync(ct)).ToHashSet(StringComparer.Ordinal);
            var group = request.GroupName ?? Enumerable.Range(1, 999).Select(n => $"N{n:00}").First(n => !used.Contains(n));
            if (used.Contains(group)) throw new CurriculumConflictException("offering_group_exists", $"Học phần đã có nhóm lớp {group} trong học kỳ {request.AcademicPeriod}.");
            var row = new CourseOffering { ProgramVersionId = versionId, ProgramCourseId = request.ProgramCourseId, AcademicPeriod = request.AcademicPeriod,
                GroupName = group, Capacity = request.Capacity, TargetHomeClassId = request.TargetHomeClassId, GradeStatus = AcademicCodes.Draft, CreatedAt = DateTime.UtcNow };
            foreach (var l in request.Lecturers ?? []) row.Lecturers.Add(new OfferingLecturer { UserId = l.UserId, Role = l.Role, AssignedAt = DateTime.UtcNow });
            db.Add(row); await db.SaveChangesAsync(ct); return row.Id;
        }, ct);
        return await OfferingAsync(versionId, id, ct);
    }
    public async Task<CourseOfferingInfo> UpdateOfferingAsync(long versionId, long offeringId, UpdateCourseOffering request, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            var row = await Lock(versionId, offeringId, ct);
            await TargetClass(versionId, request.TargetHomeClassId, ct);
            if (request.Capacity is int capacity && await db.Set<CourseEnrollment>().CountAsync(e => e.OfferingId == offeringId, ct) > capacity)
                throw new CurriculumConflictException("capacity_below_roster", "Sĩ số tối đa không được nhỏ hơn số sinh viên đang có trong lớp học phần.");
            if (request.GroupName != row.GroupName && await db.Set<CourseOffering>().AnyAsync(o => o.ProgramCourseId == row.ProgramCourseId
                    && o.AcademicPeriod == row.AcademicPeriod && o.GroupName == request.GroupName, ct))
                throw new CurriculumConflictException("offering_group_exists", $"Học phần đã có nhóm lớp {request.GroupName} trong học kỳ này.");
            row.GroupName = request.GroupName; row.Capacity = request.Capacity; row.TargetHomeClassId = request.TargetHomeClassId;
            return true;
        }, ct);
        return await OfferingAsync(versionId, offeringId, ct);
    }
    public async Task<CourseOfferingInfo> SaveLecturersAsync(long versionId, long offeringId, IReadOnlyList<OfferingLecturerInput> lecturers, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            var row = await Lock(versionId, offeringId, ct);
            row.Lecturers.Clear(); await db.SaveChangesAsync(ct);
            foreach (var l in lecturers) row.Lecturers.Add(new OfferingLecturer { UserId = l.UserId, Role = l.Role, AssignedAt = DateTime.UtcNow });
            return true;
        }, ct);
        return await OfferingAsync(versionId, offeringId, ct);
    }
    public Task<BulkEnrollResult> EnrollHomeClassAsync(long versionId, long offeringId, long homeClassId, CancellationToken ct) => writes.RunAsync(async () => {
        var offering = await Lock(versionId, offeringId, ct);
        if (offering.GradeStatus != AcademicCodes.Draft) throw new CurriculumConflictException("grades_not_draft", "Sổ điểm đã nộp hoặc đã công bố, không thể thêm sinh viên.");
        await TargetClass(versionId, homeClassId, ct);
        var students = await db.Set<AcademicStudent>().Where(s => s.HomeClassId == homeClassId).OrderBy(s => s.StudentCode).ToListAsync(ct);
        var enrolled = (await db.Set<CourseEnrollment>().Where(e => e.OfferingId == offeringId).Select(e => e.StudentId).ToArrayAsync(ct)).ToHashSet();
        var messages = new List<string>(); var candidates = new List<AcademicStudent>(); var skipped = 0;
        foreach (var s in students)
        {
            if (enrolled.Contains(s.Id)) { skipped++; continue; }
            if (await EnrollmentWriter.BlockerAsync(db, offering, s, ct) is string blocker) { skipped++; messages.Add($"{s.StudentCode}: {blocker}"); continue; }
            candidates.Add(s);
        }
        if (await EnrollmentWriter.OverCapacityAsync(db, offering, candidates.Count, ct))
            throw new CurriculumConflictException("offering_full", $"Thêm {candidates.Count} sinh viên sẽ vượt sĩ số tối đa {offering.Capacity}. Hãy tăng sĩ số hoặc mở thêm nhóm lớp.");
        foreach (var s in candidates) await EnrollmentWriter.AddAsync(db, offering, s, ct);
        return new BulkEnrollResult(candidates.Count, skipped, messages);
    }, ct);
}
