namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CreateCloRequest(
    string? Code,
    string? Statement,
    string? LevelCode);
