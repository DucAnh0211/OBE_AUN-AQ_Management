using System.Text.RegularExpressions;
using ObeAunQa.Modules.Curriculum.Domain;
namespace ObeAunQa.Modules.Curriculum.Application;

public static partial class AcademicValidator
{
    public static string Period(string? value) => Text(value, "academicPeriod", 1, 50);
    public static EnrollStudent Student(EnrollStudent request)
    {
        var code = StudentCode(request.StudentCode);
        return new(code, Text(request.FullName, "fullName", 2, 200), Period(request.AcademicPeriod));
    }
    public static string StudentCode(string? value)
    {
        var code = Text(value, "studentCode", 1, 30).ToUpperInvariant();
        if (!StudentCodePattern().IsMatch(code)) Invalid("studentCode", "Mã sinh viên chỉ gồm chữ, số, dấu chấm, gạch ngang hoặc gạch dưới.");
        return code;
    }
    public static string FullName(string? value) => Text(value, "fullName", 2, 200);

    /// <summary>Cơ cấu điểm của đề cương học phần. minComponents/minFinalWeight lấy từ quy chế của trường.</summary>
    public static void Scheme(SaveAssessmentScheme request, int minComponents = 1, decimal? minFinalWeight = null, IReadOnlyCollection<string>? cloCodes = null)
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
            if (!AcademicCodes.ComponentKinds.Contains(c.Kind)) Invalid("components", $"Loại đầu điểm của {c.Code} không hợp lệ.");
            foreach (var clo in c.Clos ?? new Dictionary<string, decimal>())
            {
                if (cloCodes is not null && !cloCodes.Contains(clo.Key)) Invalid("components", $"{clo.Key} không thuộc CLO của học phần.");
                if (clo.Value <= 0 || clo.Value > 100 || decimal.Round(clo.Value, 2) != clo.Value)
                    Invalid("components", $"Mức đóng góp của {c.Code} vào {clo.Key} phải từ 0,01 đến 100%.");
            }
        }
        if (request.Components!.Sum(c => c.Weight) != 100m) Invalid("components", "Tổng trọng số các đầu điểm phải bằng 100%.");
        if (request.Components!.Count < minComponents)
            Invalid("components", $"Quy chế của trường yêu cầu tối thiểu {minComponents} đầu điểm cho học phần này.");
        if (minFinalWeight is decimal min && request.Components!.Where(c => c.Kind == AcademicCodes.FinalExam).Sum(c => c.Weight) < min)
            Invalid("components", $"Quy chế yêu cầu điểm đánh giá cuối học phần chiếm tối thiểu {min}%.");
    }
    public static void Scores(IReadOnlyDictionary<string, decimal?>? scores, AssessmentScheme scheme, bool allowEmpty = false)
    {
        if (scores is null || (!allowEmpty && scores.Count == 0) || scores.Count > 20) Invalid("scores", "Chưa có đầu điểm cần lưu.");
        if (scores!.Count > 0 && scheme.Components.Count == 0) Invalid("scores", "Admin cần cấu hình cơ cấu điểm trong đề cương học phần trước.");
        foreach (var entry in scores)
        {
            var c = scheme.Components.SingleOrDefault(c => c.Code == entry.Key);
            if (c is null) Invalid("scores", $"Đầu điểm {entry.Key} không thuộc cơ cấu điểm của học phần.");
            if (entry.Value is decimal value && (value < 0 || value > c!.MaxScore || decimal.Round(value, 2) != value))
                Invalid("scores", $"Điểm {entry.Key} phải từ 0 đến {c!.MaxScore}, tối đa 2 chữ số thập phân.");
        }
    }
    public static string? SpecialMark(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var mark = value.Trim().ToUpperInvariant();
        if (!AcademicCodes.SpecialMarks.Contains(mark)) Invalid("specialMark", "Điểm đặc biệt chỉ nhận I (chưa hoàn thiện), R (miễn/bảo lưu) hoặc P (đạt).");
        return mark;
    }
    public static string? Reason(string? value, bool required)
    {
        if (string.IsNullOrWhiteSpace(value)) { if (required) Invalid("reason", "Cần nhập lý do (ví dụ: kết quả phúc khảo, quyết định của khoa)."); return null; }
        return Text(value, "reason", 5, 500);
    }

    public static CreateCourseOffering Offering(CreateCourseOffering request)
    {
        if (request.ProgramCourseId < 1) Invalid("programCourseId", "Chọn học phần cần mở.");
        return request with { AcademicPeriod = Period(request.AcademicPeriod), GroupName = request.GroupName is null ? null : GroupName(request.GroupName),
            Capacity = Capacity(request.Capacity), Lecturers = Lecturers(request.Lecturers ?? []) };
    }
    public static UpdateCourseOffering Offering(UpdateCourseOffering request) => request with { GroupName = GroupName(request.GroupName), Capacity = Capacity(request.Capacity) };
    public static IReadOnlyList<OfferingLecturerInput> Lecturers(IReadOnlyList<OfferingLecturerInput>? lecturers)
    {
        var list = lecturers ?? [];
        if (list.Count > 10 || list.Any(l => l is null || l.UserId < 1 || !AcademicCodes.LecturerRoles.Contains(l.Role)))
            Invalid("lecturers", "Tối đa 10 giảng viên; vai trò là giảng viên chính, trợ giảng hoặc giảng viên thực hành.");
        if (list.Select(l => l.UserId).Distinct().Count() != list.Count) Invalid("lecturers", "Mỗi giảng viên chỉ được phân công một vai trò trong lớp học phần.");
        if (list.Count > 0 && list.Count(l => l.Role == AcademicCodes.MainLecturer) != 1) Invalid("lecturers", "Lớp học phần cần đúng một giảng viên chính.");
        return list;
    }
    private static string GroupName(string? value)
    {
        var name = Text(value, "groupName", 1, 20).ToUpperInvariant();
        if (!CodePattern().IsMatch(name)) Invalid("groupName", "Nhóm lớp chỉ gồm chữ, số, dấu chấm, gạch ngang hoặc gạch dưới (ví dụ N01).");
        return name;
    }
    private static int? Capacity(int? value)
    {
        if (value is < 1 or > 1000) Invalid("capacity", "Sĩ số tối đa từ 1 đến 1000 sinh viên.");
        return value;
    }

    public static SaveOrgUnit OrgUnit(SaveOrgUnit request)
    {
        if (!AcademicCodes.OrgUnitKinds.Contains(request.Kind)) Invalid("kind", "Loại đơn vị là khoa hoặc bộ môn.");
        if (request.Kind == AcademicCodes.Department && request.ParentId is null) Invalid("parentId", "Bộ môn phải thuộc một khoa.");
        if (request.Kind == AcademicCodes.Faculty && request.ParentId is not null) Invalid("parentId", "Khoa không thuộc đơn vị nào khác.");
        return request with { Code = Code(request.Code, "code", 30), Name = Text(request.Name, "name", 2, 200) };
    }
    public static SaveLecturerProfile Lecturer(SaveLecturerProfile request)
    {
        var title = string.IsNullOrWhiteSpace(request.AcademicTitle) ? null : request.AcademicTitle.Trim();
        var degree = string.IsNullOrWhiteSpace(request.Degree) ? null : request.Degree.Trim();
        if (title is not null && !AcademicCodes.AcademicTitles.Contains(title)) Invalid("academicTitle", "Học hàm là GS hoặc PGS.");
        if (degree is not null && !AcademicCodes.Degrees.Contains(degree)) Invalid("degree", "Học vị là CN, KS, ThS, TS hoặc TSKH.");
        return new(Code(request.LecturerCode, "lecturerCode", 30), request.DepartmentId, title, degree);
    }
    public static SaveHomeClass HomeClass(SaveHomeClass request) => new(Code(request.Code, "code", 30), Text(request.Name, "name", 1, 200),
        string.IsNullOrWhiteSpace(request.CohortCode) ? null : Code(request.CohortCode, "cohortCode", 20), request.AdvisorUserId);
    public static UpdateStudent StudentProfile(UpdateStudent request)
    {
        if (!AcademicCodes.StudentStatuses.Contains(request.Status)) Invalid("status", "Trạng thái sinh viên không hợp lệ.");
        return request with { FullName = FullName(request.FullName), CohortCode = string.IsNullOrWhiteSpace(request.CohortCode) ? null : Code(request.CohortCode, "cohortCode", 20) };
    }
    public static SaveGradingPolicy Policy(SaveGradingPolicy request)
    {
        if (request.CourseScoreDecimals is < 0 or > 2 || request.GpaDecimals is < 1 or > 3) Invalid("decimals", "Điểm học phần làm tròn 0–2 chữ số, GPA làm tròn 1–3 chữ số thập phân.");
        if (request.MinComponents is < 1 or > 10) Invalid("minComponents", "Số đầu điểm tối thiểu từ 1 đến 10.");
        if (request.MinFinalWeight is decimal w && (w <= 0 || w > 100 || decimal.Round(w, 2) != w)) Invalid("minFinalWeight", "Trọng số cuối kỳ tối thiểu từ 0,01 đến 100%, hoặc để trống.");
        if (!AcademicCodes.RetakePolicies.Contains(request.RetakePolicy)) Invalid("retakePolicy", "Cách lấy điểm khi học lại là điểm cao nhất hoặc lần học gần nhất.");
        var bands = (request.Bands ?? []).Select(b => b with { Letter = (b.Letter ?? "").Trim().ToUpperInvariant() }).ToArray();
        if (bands.Any(b => !LetterPattern().IsMatch(b.Letter))) Invalid("bands", "Điểm chữ gồm 1–5 ký tự, bắt đầu bằng chữ in hoa, ví dụ A, B+.");
        var errors = GradeRules.BandErrors(bands.Select(b => new GradeBand(b.Letter, b.MinScore, b.Point4, b.IsPassing)).ToArray());
        if (errors.Count > 0) throw new CurriculumValidationException(new Dictionary<string, string[]> { ["bands"] = errors.ToArray() });
        return request with { Bands = bands };
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static void Invalid(string field, string message) => throw new CurriculumValidationException(new Dictionary<string, string[]> { [field] = [message] });
    private static string Code(string? value, string field, int max)
    {
        var code = Text(value, field, 1, max).ToUpperInvariant();
        if (!CodePattern().IsMatch(code)) Invalid(field, "Mã chỉ gồm chữ, số, dấu chấm, gạch ngang hoặc gạch dưới.");
        return code;
    }
    private static string Text(string? value, string field, int min, int max)
    {
        var text = value?.Trim() ?? "";
        if (text.Length < min || text.Length > max || text.Any(char.IsControl)) Invalid(field, $"Giá trị {field} phải từ {min} đến {max} ký tự và không chứa ký tự điều khiển.");
        return text;
    }
    [GeneratedRegex("^[A-Z0-9][A-Z0-9._-]*$", RegexOptions.CultureInvariant)] private static partial Regex StudentCodePattern();
    [GeneratedRegex("^[A-Z0-9][A-Z0-9._-]*$", RegexOptions.CultureInvariant)] private static partial Regex CodePattern();
    [GeneratedRegex("^[A-Z][A-Z0-9_]{0,19}$", RegexOptions.CultureInvariant)] private static partial Regex ComponentCodePattern();
    [GeneratedRegex("^[A-Z][A-Z+-]{0,4}$", RegexOptions.CultureInvariant)] private static partial Regex LetterPattern();
}
