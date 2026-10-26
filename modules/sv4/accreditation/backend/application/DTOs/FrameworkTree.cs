namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record FrameworkTree(long Id, string Code, string Version, string Status,
    string RequestedLanguage, string DefaultLanguage, int CriterionCount,
    int RequirementCount, IReadOnlyList<CriterionTree> Criteria);
