using System.Text.RegularExpressions;
namespace ObeAunQa.Modules.Curriculum.Application;

public static partial class AcademicValidator
{
    public static string Period(string? value) => Text(value, "academicPeriod", 1, 50);
    public static CreateAcademicClass Class(CreateAcademicClass request) => new(Period(request.AcademicPeriod),
        Text(request.Code, "code", 1, 30).ToUpperInvariant(), Text(request.Name, "name", 1, 200));
    public static EnrollStudent Student(EnrollStudent request)
    {
        var code = Text(request.StudentCode, "studentCode", 1, 30).ToUpperInvariant();
        if (!StudentCodePattern().IsMatch(code)) Invalid("studentCode", "Mã sinh viên chỉ gồm chữ, số, dấu chấm, gạch ngang hoặc gạch dưới.");
        return new(code, Text(request.FullName, "fullName", 2, 200), Period(request.AcademicPeriod));
    }
    public static void Scheme(SaveAssessmentScheme request)
    {
        if (request.Revision < 0 || request.Components is null || request.Components.Count is < 1 or > 20)
            Invalid("components", "Cần từ 1 đến 20 đầu điểm.");
        var codes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in request.Components!)
        {
            if (c is null || string.IsNullOrEmpty(c.Code) || c.Code is "MSSV" or "HOTEN" || !ComponentCodePattern().IsMatch(c.Code) || !codes.Add(c.Code))
                Invalid("components", "Mã đầu điểm phải khác nhau, từ 1–20 ký tự A–Z, 0–9, gạch dưới; bắt đầu bằng chữ.");
            Text(c!.Name, "components", 2, 100);
            if (c.Weight <= 0 || c.Weight > 100 || decimal.Round(c.Weight, 2) != c.Weight || c.MaxScore <= 0 || c.MaxScore > 1000 || decimal.Round(c.MaxScore, 2) != c.MaxScore)
                Invalid("components", "Trọng số và thang điểm phải dương, tối đa 2 chữ số thập phân (trọng số ≤100, thang điểm ≤1000).");
        }
        if (request.Components!.Sum(c => c.Weight) != 100m) Invalid("components", "Tổng trọng số các đầu điểm phải bằng 100%.");
    }
    public static void Scores(IReadOnlyDictionary<string, decimal?>? scores, AssessmentScheme scheme)
    {
        if (scores is null || scores.Count == 0 || scores.Count > 20) Invalid("scores", "Chưa có đầu điểm cần lưu.");
        if (scheme.Components.Count == 0) Invalid("scores", "Admin cần cấu hình cơ cấu điểm của phiên bản CTĐT trước.");
        foreach (var entry in scores!)
        {
            var c = scheme.Components.SingleOrDefault(c => c.Code == entry.Key);
            if (c is null) Invalid("scores", $"Đầu điểm {entry.Key} không thuộc cơ cấu điểm của CTĐT.");
            if (entry.Value is decimal value && (value < 0 || value > c!.MaxScore || decimal.Round(value, 2) != value))
                Invalid("scores", $"Điểm {entry.Key} phải từ 0 đến {c!.MaxScore}, tối đa 2 chữ số thập phân.");
        }
    }
    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static void Invalid(string field, string message) => throw new CurriculumValidationException(new Dictionary<string, string[]> { [field] = [message] });
    private static string Text(string? value, string field, int min, int max)
    {
        var text = value?.Trim() ?? "";
        if (text.Length < min || text.Length > max || text.Any(char.IsControl)) Invalid(field, $"Giá trị {field} phải từ {min} đến {max} ký tự và không chứa ký tự điều khiển.");
        return text;
    }
    [GeneratedRegex("^[A-Z0-9][A-Z0-9._-]*$", RegexOptions.CultureInvariant)] private static partial Regex StudentCodePattern();
    [GeneratedRegex("^[A-Z][A-Z0-9_]{0,19}$", RegexOptions.CultureInvariant)] private static partial Regex ComponentCodePattern();
}
