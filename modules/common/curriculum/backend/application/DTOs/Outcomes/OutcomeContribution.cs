namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeContribution(long AssessmentId, decimal? Score, decimal MaxScore, decimal EffectiveWeight);
