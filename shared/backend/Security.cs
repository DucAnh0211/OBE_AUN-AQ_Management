using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ObeAunQa.SharedKernel;

public static class SystemRoles
{
    public const string Admin = "admin";
    public const string Lecturer = "lecturer";
    public const string Student = "student";
}

public static class AuthorizationPolicies
{
    public const string Authenticated = "authenticated";
    public const string AdminOnly = "admin_only";
    public const string AdminOrLecturer = "admin_or_lecturer";
    public const string PasswordChanged = "password_changed";
}

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long UserId { get; }
    string Role { get; }
    bool MustChangePassword { get; }
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
    private ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal();
    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;
    public long UserId => long.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    public string Role => Principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    public bool MustChangePassword => string.Equals(
        Principal.FindFirstValue("must_change_password"), "true", StringComparison.OrdinalIgnoreCase);
}
