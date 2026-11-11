namespace ObeAunQa.Modules.Curriculum.Domain;

public sealed record GradeBand(string Letter, decimal MinScore, decimal Point4, bool IsPassing);

/// <summary>Result of one enrollment: score on the 10-point scale, letter grade, 4-point value and pass state.</summary>
public sealed record GradeOutcome(decimal? FinalScore, string? Letter, decimal? Point4, bool? Passed);

public static class GradeRules
{
    /// <summary>Default conversion of Thông tư 08/2021/TT-BGDĐT, Điều 10; universities may add A+/B+/C+/D+.</summary>
    public static readonly IReadOnlyList<GradeBand> DefaultBands =
    [
        new("A", 8.5m, 4m, true), new("B", 7.0m, 3m, true), new("C", 5.5m, 2m, true), new("D", 4.0m, 1m, true), new("F", 0m, 0m, false)
    ];

    public static decimal? Total(IEnumerable<(decimal? Score, decimal MaxScore, decimal Weight)> values, int decimals = 1)
    {
        var rows = values.ToArray();
        if (rows.Length == 0 || rows.Any(x => x.Score is null)) return null;
        // Normalize every component to the common ten-point final scale; missing is not zero.
        return decimal.Round(rows.Sum(x => x.Score!.Value / x.MaxScore * x.Weight) / 10m, decimals, MidpointRounding.AwayFromZero);
    }

    public static GradeBand? Band(decimal score, IEnumerable<GradeBand> bands) =>
        bands.Where(b => score >= b.MinScore).OrderByDescending(b => b.MinScore).FirstOrDefault();

    public static GradeOutcome Outcome(decimal? total, string? specialMark, IEnumerable<GradeBand> bands)
    {
        if (specialMark == "I") return new(null, "I", null, null);
        if (specialMark is "R" or "P") return new(null, specialMark, null, true);
        if (total is null) return new(null, null, null, null);
        var band = Band(total.Value, bands);
        return new(total, band?.Letter, band?.Point4, band?.IsPassing ?? false);
    }

    /// <summary>A new enrollment of a course already studied is a retake when no earlier attempt passed, otherwise an improvement.</summary>
    public static string AttemptKind(IEnumerable<bool?> earlierPassed)
    {
        var earlier = earlierPassed.ToArray();
        if (earlier.Length == 0) return AcademicCodes.FirstAttempt;
        return earlier.Any(p => p == true) ? AcademicCodes.Improve : AcademicCodes.Retake;
    }

    public static IReadOnlyList<string> BandErrors(IReadOnlyList<GradeBand> bands)
    {
        var errors = new List<string>();
        if (bands.Count is < 2 or > 15) errors.Add("Thang điểm chữ cần từ 2 đến 15 mức.");
        if (bands.Select(b => b.Letter).Distinct(StringComparer.Ordinal).Count() != bands.Count) errors.Add("Điểm chữ không được trùng.");
        if (bands.Select(b => b.MinScore).Distinct().Count() != bands.Count) errors.Add("Ngưỡng điểm của các mức không được trùng.");
        if (!bands.Any(b => b.MinScore == 0m)) errors.Add("Cần một mức bắt đầu từ 0 điểm.");
        if (bands.Any(b => b.MinScore < 0 || b.MinScore > 10 || b.Point4 < 0 || b.Point4 > 4)) errors.Add("Ngưỡng điểm từ 0 đến 10, điểm hệ 4 từ 0 đến 4.");
        var ordered = bands.OrderBy(b => b.MinScore).ToArray();
        for (var i = 1; i < ordered.Length; i++)
            if (ordered[i].Point4 < ordered[i - 1].Point4 || (!ordered[i].IsPassing && ordered[i - 1].IsPassing))
                errors.Add("Mức có ngưỡng cao hơn phải có điểm hệ 4 không thấp hơn và không được chuyển từ đạt sang không đạt.");
        return errors.Distinct().ToArray();
    }
}
