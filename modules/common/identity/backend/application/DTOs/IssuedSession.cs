namespace ObeAunQa.Modules.Identity;

public sealed record IssuedSession(LoginResponse Response,string RefreshToken,DateTimeOffset RefreshExpiresAt);
