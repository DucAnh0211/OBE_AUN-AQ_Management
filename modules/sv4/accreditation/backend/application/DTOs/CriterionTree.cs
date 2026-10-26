namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record CriterionTree(long Id, string Code, int DisplayOrder, string Title,
    string Language, string TranslationStatus, IReadOnlyList<RequirementTree> Requirements);
