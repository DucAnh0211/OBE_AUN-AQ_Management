namespace ObeAunQa.Modules.Identity;

public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);
