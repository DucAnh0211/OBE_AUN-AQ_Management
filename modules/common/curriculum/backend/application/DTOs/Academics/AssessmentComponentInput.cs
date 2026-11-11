namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AssessmentComponentInput(string Code, string Name, decimal Weight, decimal MaxScore, string Kind = "process", IReadOnlyDictionary<string, decimal>? Clos = null);
