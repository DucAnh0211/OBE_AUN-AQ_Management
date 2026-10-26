namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record FrameworkSummary(long Id, string Code, string Version,
    string AssessmentLevel, string DefaultLanguage, string Status, string SourceTitle,
    string? SourceUrl, int CriterionCount, int RequirementCount,
    DateTime? PublishedAt, DateTime? RetiredAt);
