namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record UpdateCloRequest(
    string? Code,
    string? Statement,
    string? LevelCode);
