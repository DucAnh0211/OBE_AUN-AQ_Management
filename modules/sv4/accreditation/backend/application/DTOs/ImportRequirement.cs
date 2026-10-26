namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record ImportRequirement(
    string? Code, int DisplayOrder, int? SourcePage,
    IReadOnlyDictionary<string, TranslationText>? Translations);
