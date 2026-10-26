namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CreateCoursePloMappingRequest(
    long PloId,
    string? WeightCode,
    string? ProgressionCode,
    string? Fit,
    string? FitReason);
