namespace ObeAunQa.Modules.Identity;

public sealed record UserProfile(
    long Id,
    string Email,
    string FullName,
    string Role,
    string Status,
    bool MustChangePassword,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
