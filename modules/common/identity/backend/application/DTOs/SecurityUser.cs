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
