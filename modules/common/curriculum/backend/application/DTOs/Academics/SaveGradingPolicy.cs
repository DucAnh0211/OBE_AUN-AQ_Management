namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record SaveGradingPolicy(int CourseScoreDecimals, int MinComponents, decimal? MinFinalWeight, string RetakePolicy,
    int GpaDecimals, IReadOnlyList<GradeBandInput> Bands);
