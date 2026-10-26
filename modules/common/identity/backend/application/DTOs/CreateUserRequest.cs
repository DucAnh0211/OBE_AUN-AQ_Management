namespace ObeAunQa.Modules.Identity;

public sealed record CreateUserRequest(string? Email, string? FullName, string? Role, string? TemporaryPassword);
