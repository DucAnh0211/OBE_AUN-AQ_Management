namespace ObeAunQa.Modules.Identity;

public sealed record RefreshTokenRecord(Guid Id, long UserId, string TokenHash, DateTime ExpiresAt, DateTime? RevokedAt);
