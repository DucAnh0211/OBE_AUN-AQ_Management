namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record CreateFrameworkRequest(
    string? Code, string? Version, string? AssessmentLevel, string? DefaultLanguage,
    string? SourceTitle, string? SourceUrl);
