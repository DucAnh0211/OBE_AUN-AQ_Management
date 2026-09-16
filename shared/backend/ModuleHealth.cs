namespace ObeAunQa.SharedKernel;

public sealed record ModuleHealth(
    string Module,
    string Status,
    DateTimeOffset CheckedAtUtc);
