using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Application;

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
    public async Task<PagedResult<AcademicClassInfo>> ClassesAsync(long version, string period, int page, string? search, CancellationToken ct) {
        await Version(version, ct); var q = CurriculumValidator.ListQuery(page, 50, search);
        return await repository.ClassesAsync(version, AcademicValidator.Period(period), q.Page, q.Search, Lecturer, ct);
    }
    public async Task<AcademicClassInfo> ClassAsync(long version, long id, CancellationToken ct) {
        await Version(version, ct); return await repository.ClassAsync(version, id, Lecturer, ct);
    }
    public async Task<AcademicClassInfo> CreateClassAsync(long version, CreateAcademicClass r, CancellationToken ct) {
        Admin(); return await repository.CreateClassAsync(version, AcademicValidator.Class(r), ct);
    }
    private async Task<CourseOfferingInfo> Named(CourseOfferingInfo row, CancellationToken ct) => row with { LecturerName = row.LecturerId is {} id ? (await staff.FindAsync(id, ct))?.FullName : null };
    public async Task<IReadOnlyList<CourseOfferingInfo>> OfferingsAsync(long version, long cls, CancellationToken ct) {
        await ClassAsync(version, cls, ct); var rows = await repository.OfferingsAsync(version, cls, Lecturer, ct);
        var result = new List<CourseOfferingInfo>(); foreach (var row in rows) result.Add(await Named(row, ct)); return result;
    }
    public async Task<PagedResult<CourseOfferingInfo>> SubjectOfferingsAsync(long version, long course, int page, string? search, CancellationToken ct) {
        await Version(version, ct);
        if (!await access.CanReadCourseAsync(course, ct)) throw new AcademicAccessException();
        _ = await curriculum.GetProgramCourseAsync(version, course, ct);
        var q = CurriculumValidator.ListQuery(page, 50, search);
        var result = await repository.SubjectOfferingsAsync(version, course, q.Page, q.Search, Lecturer, ct);
        var rows = new List<CourseOfferingInfo>(); foreach (var row in result.Items) rows.Add(await Named(row, ct));
        return PagedResult<CourseOfferingInfo>.Create(rows, result.Page, result.PageSize, result.TotalItems);
    }
    public async Task<CourseOfferingInfo> AccessAsync(long version, long id, bool write, CancellationToken ct) {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer)) throw new AcademicAccessException();
        var row = await repository.OfferingAsync(version, id, ct);
        if (user.Role != SystemRoles.Admin && row.LecturerId != user.UserId) throw new AcademicAccessException();
        if (write && row.ReadOnly) throw new CurriculumConflictException("archived", "Học phần hoặc phiên bản đã lưu trữ, không thể nhập thêm dữ liệu.");
        return await Named(row, ct);
    }
    private async Task Teacher(long? id, CancellationToken ct) {
        if (id.HasValue && await staff.FindAsync(id.Value, ct) is null) AcademicValidator.Invalid("lecturerId", "Chọn tài khoản giảng viên đang hoạt động.");
    }
    public async Task<CourseOfferingInfo> CreateOfferingAsync(long version, long cls, CreateCourseOffering request, CancellationToken ct) {
        Admin(); await Teacher(request.LecturerId, ct); return await Named(await repository.CreateOfferingAsync(version, cls, request, ct), ct);
    }
    public async Task<CourseOfferingInfo> AssignAsync(long version, long id, long? lecturer, CancellationToken ct) {
        Admin(); await AccessAsync(version, id, true, ct); await Teacher(lecturer, ct);
        return await Named(await repository.AssignLecturerAsync(version, id, lecturer, ct), ct);
    }
    public Task<IReadOnlyList<AcademicStaffMember>> LecturersAsync(string? search, CancellationToken ct) { Admin(); return staff.SearchAsync(search, ct); }
    public async Task<PagedResult<ProgramCourseResponse>> SubjectsAsync(long version, long cls, int page, string? search, CancellationToken ct) {
        Admin(); await repository.ClassAsync(version, cls, null, ct);
        return await curriculum.GetProgramCoursesAsync(version, page, 50, search, null, false, ct);
    }
}
