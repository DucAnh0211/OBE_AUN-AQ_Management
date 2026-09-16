namespace ObeAunQa.SharedKernel;

public sealed record ModuleDescriptor(
    string Key,
    string DisplayName,
    string Owner,
    string ApiPrefix,
    string Description);
