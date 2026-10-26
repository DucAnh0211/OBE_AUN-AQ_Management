namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record RequirementTree(long Id, string Code, int DisplayOrder, int? SourcePage,
    string Statement, string Language, string TranslationStatus);
