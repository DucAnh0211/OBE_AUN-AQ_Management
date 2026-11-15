using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ObeAunQa.SharedKernel;

public static class SystemRoles
{
    public const string Admin = "admin";
    public const string SchoolAdmin = "school_admin";
    public static bool IsAdmin(string role) => role is Admin or SchoolAdmin;
    public const string Lecturer = "lecturer";
    public const string Student = "student";
}

public static class AuthorizationPolicies
{
    public const string CurriculumManagers = "curriculum_managers";
    public const string Authenticated = "authenticated";
    public const string AdminOnly = "admin_only";
    public const string GlobalAdminOnly = "global_admin_only";
    public const string AdminOrLecturer = "admin_or_lecturer";
    public const string PasswordChanged = "password_changed";
}

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long UserId { get; }
    string Role { get; }
    bool MustChangePassword { get; }
    long? UniversityId => null;
    bool IsSystemContext => false;
}

public interface IAccessControlService
{
    Task<bool> CanReadProgramAsync(long programId, CancellationToken cancellationToken);
    Task<bool> CanReadVersionAsync(long versionId, CancellationToken cancellationToken);
    Task<bool> CanReadCourseAsync(long programCourseId, CancellationToken cancellationToken);
    Task<bool> CanEditCourseOutcomesAsync(long programCourseId, CancellationToken cancellationToken);
    Task<long[]> GetReadableProgramIdsAsync(CancellationToken cancellationToken);
    Task<long[]> GetReadableCourseIdsAsync(long versionId, CancellationToken cancellationToken);
    Task<bool> CanReadFrameworkAsync(long frameworkId, CancellationToken cancellationToken);
}

public interface IAssignmentCloneService
{
    Task CopyLecturerAssignmentsAsync(long sourceVersionId,long targetVersionId,CancellationToken cancellationToken);
}

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private (long Id, string Role, long? UniversityId)? execution;
    private ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal();
    public bool IsAuthenticated => execution != null || Principal.Identity?.IsAuthenticated == true;
    public long UserId => execution?.Id ?? (long.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0);
    public string Role => execution?.Role ?? Principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    public bool IsSystemContext => accessor.HttpContext == null && execution == null;
    public long? UniversityId => execution?.UniversityId ?? (Role == SystemRoles.Admin
        && long.TryParse(accessor.HttpContext?.Request.Headers["X-University-Id"], out var selected) && selected > 0 ? selected
        : long.TryParse(Principal.FindFirstValue("university_id"), out var school) ? school : null);
    public IDisposable ExecuteAs(long id, string role, long? universityId)
    {
        var previous = execution; execution = (id, role, universityId);
        return new Restore(() => execution = previous);
    }
    private sealed class Restore(Action restore) : IDisposable { public void Dispose() => restore(); }
    public bool MustChangePassword => string.Equals(
        Principal.FindFirstValue("must_change_password"), "true", StringComparison.OrdinalIgnoreCase);
}
