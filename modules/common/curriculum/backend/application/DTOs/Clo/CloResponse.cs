namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CloResponse(
    long Id,
    long ProgramCourseId,
    string Code,
    string Statement,
    string? LevelCode,
    string Provenance,
    string Status);
