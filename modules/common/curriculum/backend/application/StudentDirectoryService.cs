using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Application;

/// <summary>Lớp sinh hoạt, hồ sơ sinh viên và bảng điểm/GPA theo quy chế của trường.</summary>
public sealed class StudentDirectoryService(IStudentDirectoryRepository repository, CurriculumService curriculum, UniversityService universities,
    ICurrentUser user, IAccessControlService access, IAcademicStaffDirectory staff)
{
    private bool IsAdmin => user.Role == SystemRoles.Admin;
    private void Admin() { if (!IsAdmin) throw new AcademicAccessException(); }
    private async Task Version(long id, CancellationToken ct)
    {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer) || !await access.CanReadVersionAsync(id, ct)) throw new AcademicAccessException();
        _ = await curriculum.GetProgramVersionAsync(id, ct);
    }
    /// <summary>Admin và giảng viên đọc được phiên bản thấy mọi lớp; cố vấn học tập luôn thấy lớp mình phụ trách.</summary>
    private async Task<long?> AdvisorScope(long versionId, CancellationToken ct)
    {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer)) throw new AcademicAccessException();
        _ = await curriculum.GetProgramVersionAsync(versionId, ct);
        return IsAdmin || await access.CanReadVersionAsync(versionId, ct) ? null : user.UserId;
    }
    public async Task<PagedResult<HomeClassInfo>> HomeClassesAsync(long versionId, int page, string? search, CancellationToken ct)
    {
        var advisor = await AdvisorScope(versionId, ct); var q = CurriculumValidator.ListQuery(page, 50, search);
        var result = await repository.HomeClassesAsync(versionId, q.Page, q.Search, advisor, ct);
        return PagedResult<HomeClassInfo>.Create(await Named(result.Items, ct), result.Page, result.PageSize, result.TotalItems);
    }
    public async Task<HomeClassInfo> HomeClassAsync(long versionId, long id, CancellationToken ct)
    {
        var advisor = await AdvisorScope(versionId, ct);
        var row = await repository.HomeClassAsync(versionId, id, ct);
        if (advisor is not null && row.AdvisorUserId != advisor) throw new AcademicAccessException();
        return (await Named([row], ct))[0];
    }
    public async Task<HomeClassInfo> SaveHomeClassAsync(long versionId, long? id, SaveHomeClass request, CancellationToken ct)
    {
        Admin(); await Version(versionId, ct);
        var input = AcademicValidator.HomeClass(request);
        if (input.AdvisorUserId is long advisor && await staff.FindAsync(advisor, ct) is null) AcademicValidator.Invalid("advisorUserId", "Cố vấn học tập phải là tài khoản giảng viên đang hoạt động.");
        return (await Named([await repository.SaveHomeClassAsync(versionId, id, input, ct)], ct))[0];
    }
    public async Task<StudentInfo> AddToHomeClassAsync(long versionId, long homeClassId, AddHomeClassStudent request, CancellationToken ct)
    {
        Admin(); await Version(versionId, ct);
        return await repository.AddToHomeClassAsync(versionId, homeClassId, new(AcademicValidator.StudentCode(request.StudentCode), AcademicValidator.FullName(request.FullName)), ct);
    }
    public async Task<AcademicImportPreview> ImportHomeClassAsync(long versionId, long homeClassId, HomeClassImportRequest request, bool apply, CancellationToken ct)
    {
        Admin(); await Version(versionId, ct);
        if (request.Rows is null || request.Rows.Count is < 1 or > 500 || request.Rows.Any(r => r is null)) AcademicValidator.Invalid("rows", "Mỗi lần import từ 1 đến 500 sinh viên.");
        return await repository.ImportHomeClassAsync(versionId, homeClassId, request.Rows!, apply, ct);
    }
    /// <summary>Admin xem toàn trường; cố vấn học tập chỉ xem sinh viên lớp sinh hoạt mình phụ trách.</summary>
    public async Task<PagedResult<StudentInfo>> StudentsAsync(long universityId, int page, string? search, long? homeClassId, long? versionId, CancellationToken ct)
    {
        await universities.GetUniversityAsync(universityId, ct);
        if (!IsAdmin)
        {
            if (user.Role != SystemRoles.Lecturer || homeClassId is null || versionId is null) throw new AcademicAccessException();
            if ((await repository.HomeClassAsync(versionId.Value, homeClassId.Value, ct)).AdvisorUserId != user.UserId) throw new AcademicAccessException();
        }
        var q = CurriculumValidator.ListQuery(page, 50, search);
        return await repository.StudentsAsync(universityId, q.Page, q.Search, homeClassId, ct);
    }
    public async Task<StudentInfo> UpdateStudentAsync(long universityId, long id, UpdateStudent request, CancellationToken ct)
    {
        Admin(); var input = AcademicValidator.StudentProfile(request);
        if (input.UserId is long account && await staff.FindAccountAsync(account, SystemRoles.Student, ct) is null)
            AcademicValidator.Invalid("userId", "Chỉ liên kết với tài khoản sinh viên đang hoạt động.");
        return await repository.UpdateStudentAsync(universityId, id, input, ct);
    }
    public Task<IReadOnlyList<AcademicStaffMember>> StudentAccountsAsync(string? search, CancellationToken ct) { Admin(); return staff.SearchAccountsAsync(SystemRoles.Student, search, ct); }
    public async Task<StudentTranscript> TranscriptAsync(long universityId, long studentId, CancellationToken ct)
    {
        var student = await repository.StudentAsync(universityId, studentId, ct);
        var allowed = IsAdmin
            || (user.Role == SystemRoles.Lecturer && await repository.AdvisorOfStudentAsync(studentId, ct) == user.UserId)
            || (user.Role == SystemRoles.Student && student.UserId == user.UserId);
        if (!allowed) throw new AcademicAccessException();
        return await repository.TranscriptAsync(student.Id, ct);
    }
    public async Task<StudentTranscript> MyTranscriptAsync(CancellationToken ct)
    {
        if (user.Role != SystemRoles.Student) throw new AcademicAccessException();
        var student = await repository.StudentByUserAsync(user.UserId, ct)
            ?? throw new CurriculumConflictException("student_not_linked", "Tài khoản chưa được liên kết với hồ sơ sinh viên. Liên hệ phòng đào tạo.");
        return await repository.TranscriptAsync(student.Id, ct);
    }
    private async Task<IReadOnlyList<HomeClassInfo>> Named(IReadOnlyList<HomeClassInfo> rows, CancellationToken ct)
    {
        var people = (await staff.FindManyAsync(rows.Where(r => r.AdvisorUserId != null).Select(r => r.AdvisorUserId!.Value).ToArray(), ct)).ToDictionary(p => p.Id);
        return rows.Select(r => r.AdvisorUserId is long a && people.TryGetValue(a, out var p) ? r with { AdvisorName = p.FullName } : r).ToArray();
    }
}
