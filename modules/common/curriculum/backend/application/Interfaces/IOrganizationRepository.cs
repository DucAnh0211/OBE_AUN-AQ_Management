namespace ObeAunQa.Modules.Curriculum.Application;

public interface IOrganizationRepository
{
    Task<IReadOnlyList<OrgUnitInfo>> OrgUnitsAsync(long universityId, bool includeArchived, CancellationToken ct);
    Task<OrgUnitInfo> SaveOrgUnitAsync(long universityId, long? id, SaveOrgUnit request, CancellationToken ct);
    Task ArchiveOrgUnitAsync(long universityId, long id, CancellationToken ct);
    /// <summary>Profiles of the given lecturer accounts; names are filled by the caller from the identity directory.</summary>
    Task<IReadOnlyList<LecturerProfileInfo>> ProfilesAsync(IReadOnlyCollection<long> userIds, CancellationToken ct);
    Task SaveProfileAsync(long userId, SaveLecturerProfile request, CancellationToken ct);
    Task<GradingPolicyInfo> PolicyAsync(long universityId, CancellationToken ct);
    Task<GradingPolicyInfo> SavePolicyAsync(long universityId, SaveGradingPolicy request, CancellationToken ct);
    Task AssignFacultyAsync(long programId, long? facultyId, CancellationToken ct);
    Task AssignCourseCatalogAsync(long versionId, long programCourseId, UpdateCourseCatalog request, CancellationToken ct);
}
