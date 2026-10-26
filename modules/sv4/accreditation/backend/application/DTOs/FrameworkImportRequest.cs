namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record FrameworkImportRequest(
    string? Code, string? Version, string? AssessmentLevel, string? DefaultLanguage,
    string? SourceTitle, string? SourceUrl, IReadOnlyList<ImportCriterion>? Criteria);
