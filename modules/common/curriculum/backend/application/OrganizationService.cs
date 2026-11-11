using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Application;

/// <summary>Khoa/bộ môn, hồ sơ giảng viên và quy chế đánh giá của trường.</summary>
public sealed class OrganizationService(IOrganizationRepository repository, ICurrentUser user, IAcademicStaffDirectory staff, UniversityService universities)
{
    private void Admin() { if (user.Role != SystemRoles.Admin) throw new AcademicAccessException(); }
    private void Staff() { if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer)) throw new AcademicAccessException(); }
    public async Task<IReadOnlyList<OrgUnitInfo>> OrgUnitsAsync(long universityId, bool includeArchived, CancellationToken ct)
    {
        Staff(); await universities.GetUniversityAsync(universityId, ct);
        return await repository.OrgUnitsAsync(universityId, includeArchived && user.Role == SystemRoles.Admin, ct);
    }
    public async Task<OrgUnitInfo> SaveOrgUnitAsync(long universityId, long? id, SaveOrgUnit request, CancellationToken ct)
    {
        Admin(); await universities.GetUniversityAsync(universityId, ct);
        return await repository.SaveOrgUnitAsync(universityId, id, AcademicValidator.OrgUnit(request), ct);
    }
    public Task ArchiveOrgUnitAsync(long universityId, long id, CancellationToken ct) { Admin(); return repository.ArchiveOrgUnitAsync(universityId, id, ct); }
    public async Task<IReadOnlyList<LecturerProfileInfo>> LecturersAsync(string? search, CancellationToken ct)
    {
        Admin();
        var accounts = await staff.SearchAsync(search, ct);
        var profiles = (await repository.ProfilesAsync(accounts.Select(a => a.Id).ToArray(), ct)).ToDictionary(p => p.UserId);
        return accounts.Select(a => profiles.TryGetValue(a.Id, out var p) ? p with { FullName = a.FullName, Email = a.Email }
            : new LecturerProfileInfo(a.Id, a.FullName, a.Email, null, null, null, null, null)).ToArray();
    }
    public async Task<LecturerProfileInfo> SaveLecturerAsync(long userId, SaveLecturerProfile request, CancellationToken ct)
    {
        Admin();
        var account = await staff.FindAsync(userId, ct) ?? throw new CurriculumNotFoundException("tài khoản giảng viên đang hoạt động", userId);
        await repository.SaveProfileAsync(userId, AcademicValidator.Lecturer(request), ct);
        var saved = (await repository.ProfilesAsync([userId], ct)).Single();
        return saved with { FullName = account.FullName, Email = account.Email };
    }
    public async Task<GradingPolicyInfo> PolicyAsync(long universityId, CancellationToken ct)
    {
        Staff(); await universities.GetUniversityAsync(universityId, ct);
        return await repository.PolicyAsync(universityId, ct);
    }
    public async Task<GradingPolicyInfo> SavePolicyAsync(long universityId, SaveGradingPolicy request, CancellationToken ct)
    {
        Admin(); await universities.GetUniversityAsync(universityId, ct);
        return await repository.SavePolicyAsync(universityId, AcademicValidator.Policy(request), ct);
    }
    public Task AssignFacultyAsync(long programId, AssignOrgUnit request, CancellationToken ct) { Admin(); return repository.AssignFacultyAsync(programId, request.OrgUnitId, ct); }
    public Task AssignCourseCatalogAsync(long versionId, long programCourseId, UpdateCourseCatalog request, CancellationToken ct)
    {
        Admin(); return repository.AssignCourseCatalogAsync(versionId, programCourseId, request, ct);
    }
}
