namespace ObeAunQa.Modules.Curriculum.Domain;

public static class CurriculumAnalysisRules
{
    public const int MaximumRequiredPloCount = 5;
    public const decimal AllowedDeviation = 0.20m;
    public static int RequiredPlos(int credits) => Math.Min(credits, MaximumRequiredPloCount);
    public static decimal? Deviation(decimal score, decimal mean) => mean == 0 ? null : (score - mean) / mean;
    public static bool ExceedsLimit(decimal? deviation) => deviation.HasValue && Math.Abs(deviation.Value) > AllowedDeviation;
}
