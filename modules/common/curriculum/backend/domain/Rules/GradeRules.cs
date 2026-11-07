namespace ObeAunQa.Modules.Curriculum.Domain;

public static class GradeRules
{
    public static decimal? Total(IEnumerable<(decimal? Score, decimal MaxScore, decimal Weight)> values)
    {
        var rows = values.ToArray();
        if (rows.Length == 0 || rows.Any(x => x.Score is null)) return null;
        // Normalize every component to the common ten-point final scale; missing is not zero.
        return decimal.Round(rows.Sum(x => x.Score!.Value / x.MaxScore * x.Weight) / 10m, 2, MidpointRounding.AwayFromZero);
    }
}
