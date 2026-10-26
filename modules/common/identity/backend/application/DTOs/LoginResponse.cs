namespace ObeAunQa.Modules.Identity;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, UserProfile User);
