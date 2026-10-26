namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record ImportValidationResult(bool Valid, string? Code, string? Version,
    int CriterionCount, int RequirementCount, IReadOnlyDictionary<string, string[]> Errors);
