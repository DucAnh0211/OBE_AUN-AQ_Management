namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record CriterionRequest(string? Code, int DisplayOrder,
    IReadOnlyDictionary<string, TranslationText>? Translations = null);
