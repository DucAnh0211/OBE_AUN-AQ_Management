namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeAssessment(long Id, string Code, decimal Weight, decimal MaxScore, IReadOnlyDictionary<long, decimal> Clos);
