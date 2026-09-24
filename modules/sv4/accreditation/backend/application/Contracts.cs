namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record CreateFrameworkRequest(
    string? Code, string? Version, string? AssessmentLevel, string? DefaultLanguage,
    string? SourceTitle, string? SourceUrl);
public sealed record UpdateFrameworkRequest(
    string? DefaultLanguage, string? SourceTitle, string? SourceUrl);
public sealed record CriterionRequest(string? Code, int DisplayOrder,
    IReadOnlyDictionary<string, TranslationText>? Translations = null);
public sealed record RequirementRequest(string? Code, int DisplayOrder, int? SourcePage,
    IReadOnlyDictionary<string, TranslationText>? Translations = null);
public sealed record TranslationText(string? Title, string? Statement, string? Description,
    string? Guidance, string? Status, string? Source);
public sealed record FrameworkImportRequest(
    string? Code, string? Version, string? AssessmentLevel, string? DefaultLanguage,
    string? SourceTitle, string? SourceUrl, IReadOnlyList<ImportCriterion>? Criteria);
public sealed record ImportCriterion(
    string? Code, int DisplayOrder, IReadOnlyDictionary<string, TranslationText>? Translations,
    IReadOnlyList<ImportRequirement>? Requirements);
public sealed record ImportRequirement(
    string? Code, int DisplayOrder, int? SourcePage,
    IReadOnlyDictionary<string, TranslationText>? Translations);

public sealed record FrameworkSummary(long Id, string Code, string Version,
    string AssessmentLevel, string DefaultLanguage, string Status, string SourceTitle,
    string? SourceUrl, int CriterionCount, int RequirementCount,
    DateTime? PublishedAt, DateTime? RetiredAt);
public sealed record FrameworkTree(long Id, string Code, string Version, string Status,
    string RequestedLanguage, string DefaultLanguage, int CriterionCount,
    int RequirementCount, IReadOnlyList<CriterionTree> Criteria);
public sealed record CriterionTree(long Id, string Code, int DisplayOrder, string Title,
    string Language, string TranslationStatus, IReadOnlyList<RequirementTree> Requirements);
public sealed record RequirementTree(long Id, string Code, int DisplayOrder, int? SourcePage,
    string Statement, string Language, string TranslationStatus);
public sealed record ImportValidationResult(bool Valid, string? Code, string? Version,
    int CriterionCount, int RequirementCount, IReadOnlyDictionary<string, string[]> Errors);

public sealed class AccreditationValidationException(
    string message, IReadOnlyDictionary<string, string[]> errors) : Exception(message)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
public sealed class AccreditationNotFoundException(string message) : Exception(message);
public sealed class AccreditationConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
