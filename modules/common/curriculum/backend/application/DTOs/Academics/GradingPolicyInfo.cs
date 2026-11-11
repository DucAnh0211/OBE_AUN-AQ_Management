namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record GradingPolicyInfo(long UniversityId, int CourseScoreDecimals, int MinComponents, decimal? MinFinalWeight,
    string RetakePolicy, int GpaDecimals, IReadOnlyList<GradeBandInput> Bands);
