namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CoursePloMappingResponse(
    long Id,
    long ProgramVersionId,
    long ProgramCourseId,
    long PloId,
    string PloCode,
    string WeightCode,
    string ProgressionCode,
    string Provenance,
    string? Fit,
    string? FitReason);
