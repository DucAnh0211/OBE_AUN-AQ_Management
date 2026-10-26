namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record ImportCriterion(
    string? Code, int DisplayOrder, IReadOnlyDictionary<string, TranslationText>? Translations,
    IReadOnlyList<ImportRequirement>? Requirements);
