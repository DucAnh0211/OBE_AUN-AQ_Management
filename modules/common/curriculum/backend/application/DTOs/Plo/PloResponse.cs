namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record PloResponse(
    long Id,
    long ProgramVersionId,
    string Code,
    string Statement,
    string? LevelCode,
    string Provenance);
