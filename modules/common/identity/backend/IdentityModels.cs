namespace ObeAunQa.Modules.Identity;

public sealed record SecurityUser(
    long Id,
    string Email,
    string NormalizedEmail,
    string FullName,
    string PasswordHash,
    string Role,
    string Status,
    bool MustChangePassword,
    int TokenVersion,
    int FailedLoginCount,
    DateTime? LockedUntil,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record UserProfile(
    long Id,
    string Email,
    string FullName,
    string Role,
    string Status,
    bool MustChangePassword,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public sealed record LoginRequest(string? Email, string? Password);
public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);
public sealed record CreateUserRequest(string? Email, string? FullName, string? Role, string? TemporaryPassword);
public sealed record UpdateUserRequest(string? FullName, string? Role, string? Status);
public sealed record ResetPasswordRequest(string? TemporaryPassword);
public sealed record AssignmentRequest(long[]? Ids);
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, UserProfile User);
public sealed record PagedUsers(IReadOnlyList<UserProfile> Items, int Page, int PageSize, long TotalItems, int TotalPages);
public sealed record AuditLogResponse(long Id, long? ActorUserId, string Action, string TargetType, string? TargetId, string Details, string? IpAddress, DateTime CreatedAt);
public sealed record PagedAuditLogs(IReadOnlyList<AuditLogResponse> Items, int Page, int PageSize, long TotalItems, int TotalPages);
public sealed record RefreshTokenRecord(Guid Id, long UserId, string TokenHash, DateTime ExpiresAt, DateTime? RevokedAt);

public sealed class IdentityApiException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "ObeAunQa.Api";
    public string Audience { get; set; } = "ObeAunQa.Web";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = "Quản trị viên";
    public string Password { get; set; } = string.Empty;
}
