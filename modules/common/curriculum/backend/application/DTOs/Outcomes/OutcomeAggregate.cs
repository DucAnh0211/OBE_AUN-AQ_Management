namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeAggregate(long OutcomeId, string Code, int EligibleCount, int EvaluatedCount,
    int AchievedCount, int MissingCount, int ExcludedCount, decimal? AchievementRate,
    decimal? DataCoverageRate, decimal? MeanScore, string Status);
