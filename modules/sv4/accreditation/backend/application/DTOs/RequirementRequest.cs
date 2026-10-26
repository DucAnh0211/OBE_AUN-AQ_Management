namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record RequirementRequest(string? Code, int DisplayOrder, int? SourcePage,
    IReadOnlyDictionary<string, TranslationText>? Translations = null);
