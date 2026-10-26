namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record UpdatePloRequest(
    string? Code,
    string? Statement,
    string? LevelCode);
