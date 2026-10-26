namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record NormalizedLearningOutcome(
    string Code,
    string Statement,
    string? LevelCode);
