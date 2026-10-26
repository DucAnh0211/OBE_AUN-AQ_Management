namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record UpdateCoursePloMappingRequest(
    string? WeightCode,
    string? ProgressionCode,
    string? Fit,
    string? FitReason);
