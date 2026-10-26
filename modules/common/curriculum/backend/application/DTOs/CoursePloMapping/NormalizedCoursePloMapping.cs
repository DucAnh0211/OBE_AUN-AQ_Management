namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record NormalizedCoursePloMapping(
    long PloId,
    string WeightCode,
    string ProgressionCode,
    string? Fit,
    string? FitReason);
