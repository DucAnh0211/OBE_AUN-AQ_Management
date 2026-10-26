namespace ObeAunQa.Modules.Identity;

public sealed record UpdateUserRequest(string? FullName, string? Role, string? Status);
