namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record NormalizedCoursePloMappingUpdate(
    string WeightCode,
    string ProgressionCode,
    string? Fit,
    string? FitReason);
