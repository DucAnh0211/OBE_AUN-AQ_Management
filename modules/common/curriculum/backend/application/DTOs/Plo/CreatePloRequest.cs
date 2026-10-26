namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CreatePloRequest(
    string? Code,
    string? Statement,
    string? LevelCode);
