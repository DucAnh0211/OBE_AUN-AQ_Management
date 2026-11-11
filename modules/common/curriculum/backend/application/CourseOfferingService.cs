using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Application;

/// <summary>Lớp học phần: mở theo học phần × học kỳ, phân công giảng viên, nhận sinh viên từ lớp sinh hoạt.</summary>
public sealed class CourseOfferingService(ICourseOfferingRepository repository, CurriculumService curriculum,
    ICurrentUser user, IAccessControlService access, IAcademicStaffDirectory staff)
{
    private long? Lecturer => user.Role == SystemRoles.Admin ? null : user.UserId;
    private void Admin() { if (user.Role != SystemRoles.Admin) throw new AcademicAccessException(); }
    private async Task Version(long id, CancellationToken ct) {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer) || !await access.CanReadVersionAsync(id, ct)) throw new AcademicAccessException();
        _ = await curriculum.GetProgramVersionAsync(id, ct);
    }
    public async Task<IReadOnlyList<AcademicPeriodBatch>> PeriodsAsync(long version, CancellationToken ct) {
        await Version(version, ct); return AcademicPeriodCatalog.Build(await repository.PeriodsAsync(version, Lecturer, ct), DateTime.UtcNow.Year);
    }
    public async Task<PagedResult<CourseOfferingInfo>> OfferingsAsync(long version, string period, int page, string? search, CancellationToken ct) {
        await Version(version, ct); var q = CurriculumValidator.ListQuery(page, 50, search);
        return await Named(await repository.OfferingsAsync(version, AcademicValidator.Period(period), q.Page, q.Search, Lecturer, ct), ct);
    }
    public async Task<PagedResult<CourseOfferingInfo>> SubjectOfferingsAsync(long version, long course, int page, string? search, CancellationToken ct) {
        await Version(version, ct);
        if (!await access.CanReadCourseAsync(course, ct)) throw new AcademicAccessException();
        _ = await curriculum.GetProgramCourseAsync(version, course, ct);
        var q = CurriculumValidator.ListQuery(page, 50, search);
        return await Named(await repository.SubjectOfferingsAsync(version, course, q.Page, q.Search, Lecturer, ct), ct);
    }
    /// <summary>Admin hoặc giảng viên được phân công (mọi vai trò) mới đọc được; ghi kiểm tra thêm trạng thái lưu trữ.</summary>
    public async Task<CourseOfferingInfo> AccessAsync(long version, long id, bool write, CancellationToken ct) {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer)) throw new AcademicAccessException();
        var row = await repository.OfferingAsync(version, id, ct);
        if (user.Role != SystemRoles.Admin && row.Lecturers.All(l => l.UserId != user.UserId)) throw new AcademicAccessException();
        if (write && row.ReadOnly) throw new CurriculumConflictException("archived", "Học phần hoặc phiên bản đã lưu trữ, không thể nhập thêm dữ liệu.");
        return await Named(row, ct);
    }
    public async Task<CourseOfferingInfo> CreateOfferingAsync(long version, CreateCourseOffering request, CancellationToken ct) {
        Admin(); var input = AcademicValidator.Offering(request); await Teachers(input.Lecturers!, ct);
        return await Named(await repository.CreateOfferingAsync(version, input, ct), ct);
    }
    public async Task<CourseOfferingInfo> UpdateOfferingAsync(long version, long id, UpdateCourseOffering request, CancellationToken ct) {
        Admin(); await AccessAsync(version, id, true, ct);
        return await Named(await repository.UpdateOfferingAsync(version, id, AcademicValidator.Offering(request), ct), ct);
    }
    public async Task<CourseOfferingInfo> SaveLecturersAsync(long version, long id, SaveOfferingLecturers request, CancellationToken ct) {
        Admin(); await AccessAsync(version, id, true, ct);
        var lecturers = AcademicValidator.Lecturers(request.Lecturers); await Teachers(lecturers, ct);
        return await Named(await repository.SaveLecturersAsync(version, id, lecturers, ct), ct);
    }
    public async Task<BulkEnrollResult> EnrollHomeClassAsync(long version, long id, EnrollHomeClass request, CancellationToken ct) {
        Admin(); var offering = await AccessAsync(version, id, true, ct);
        if (offering.GradeStatus != AcademicCodes.Draft) throw new CurriculumConflictException("grades_not_draft", "Lớp học phần đã nộp hoặc công bố điểm, không thể thêm sinh viên.");
        return await repository.EnrollHomeClassAsync(version, id, request.HomeClassId, ct);
    }
    public Task<IReadOnlyList<AcademicStaffMember>> LecturersAsync(string? search, CancellationToken ct) { Admin(); return staff.SearchAsync(search, ct); }
    public async Task<PagedResult<ProgramCourseResponse>> SubjectsAsync(long version, int page, string? search, CancellationToken ct) {
        Admin(); return await curriculum.GetProgramCoursesAsync(version, page, 50, search, null, false, ct);
    }
    private async Task Teachers(IReadOnlyList<OfferingLecturerInput> lecturers, CancellationToken ct) {
        foreach (var l in lecturers)
            if (await staff.FindAsync(l.UserId, ct) is null) AcademicValidator.Invalid("lecturers", $"Tài khoản {l.UserId} không phải giảng viên đang hoạt động.");
    }
    private async Task<CourseOfferingInfo> Named(CourseOfferingInfo row, CancellationToken ct) => (await Named([row], ct))[0];
    private async Task<PagedResult<CourseOfferingInfo>> Named(PagedResult<CourseOfferingInfo> page, CancellationToken ct) =>
        PagedResult<CourseOfferingInfo>.Create(await Named(page.Items, ct), page.Page, page.PageSize, page.TotalItems);
    private async Task<IReadOnlyList<CourseOfferingInfo>> Named(IReadOnlyList<CourseOfferingInfo> rows, CancellationToken ct) {
        var people = (await staff.FindManyAsync(rows.SelectMany(r => r.Lecturers.Select(l => l.UserId)).ToArray(), ct)).ToDictionary(p => p.Id);
        return rows.Select(r => r with { Lecturers = r.Lecturers.OrderBy(l => l.Role != AcademicCodes.MainLecturer).ThenBy(l => l.UserId)
            .Select(l => people.TryGetValue(l.UserId, out var p) ? l with { FullName = p.FullName, Email = p.Email } : l).ToArray() }).ToArray();
    }
}
