namespace ObeAunQa.Modules.Curriculum.Domain;

/// <summary>One published course result used for GPA; CourseId identifies the catalog course across program versions.</summary>
public sealed record TranscriptAttempt(long CourseId, string Period, int Credits, bool CountsTowardGpa, decimal? Point4, bool? Passed);

public sealed record GpaSummary(decimal? Gpa, int GpaCredits, int EarnedCredits);

public static class GpaRules
{
    public static GpaSummary Semester(IEnumerable<TranscriptAttempt> rows, int decimals = 2) => Summarize(rows.ToArray(), decimals);

    /// <summary>Cumulative GPA keeps one attempt per course according to the university's retake policy.</summary>
    public static GpaSummary Cumulative(IEnumerable<TranscriptAttempt> rows, string retakePolicy, int decimals = 2) =>
        Summarize(rows.GroupBy(r => r.CourseId).Select(g => Chosen(g, retakePolicy)).OfType<TranscriptAttempt>().ToArray(), decimals);

    public static TranscriptAttempt? Chosen(IEnumerable<TranscriptAttempt> attempts, string retakePolicy)
    {
        var graded = attempts.Where(a => a.Passed is not null).ToArray();
        if (graded.Length == 0) return null;
        var latest = graded.OrderByDescending(a => a.Period, StringComparer.Ordinal);
        return retakePolicy == AcademicCodes.Latest ? latest.First()
            : latest.OrderByDescending(a => a.Point4 ?? (a.Passed == true ? 0m : -1m)).ThenByDescending(a => a.Passed == true).First();
    }

    /// <summary>Xếp loại học lực theo điểm trung bình tích lũy hệ 4 (Thông tư 08/2021/TT-BGDĐT, Điều 10).</summary>
    public static string? Classification(decimal? gpa) => gpa switch
    {
        null => null,
        >= 3.6m => "Xuất sắc",
        >= 3.2m => "Giỏi",
        >= 2.5m => "Khá",
        >= 2.0m => "Trung bình",
        >= 1.0m => "Yếu",
        _ => "Kém"
    };

    private static GpaSummary Summarize(TranscriptAttempt[] rows, int decimals)
    {
        var gpaRows = rows.Where(r => r.CountsTowardGpa && r.Point4 is not null && r.Credits > 0).ToArray();
        var credits = gpaRows.Sum(r => r.Credits);
        decimal? gpa = credits == 0 ? null : decimal.Round(gpaRows.Sum(r => r.Point4!.Value * r.Credits) / credits, decimals, MidpointRounding.AwayFromZero);
        return new(gpa, credits, rows.Where(r => r.Passed == true).Sum(r => r.Credits));
    }
}
