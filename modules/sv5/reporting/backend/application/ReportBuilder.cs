using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public static class ReportBuilder
{
    public const string RuleVersion = "curriculum-reports/1.0";
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Validate(CreateReportRequest request)
    {
        if (request.Kind is not ("course" or "clo" or "plo"))
            throw new ReportException(400, "Loại báo cáo phải là course, clo hoặc plo.");
        if (request.VersionId <= 0 || request.ProgramCourseId is <= 0)
            throw new ReportException(400, "Mã phiên bản/học phần không hợp lệ.");
        if (request.Kind == "course" && request.ProgramCourseId is null)
            throw new ReportException(400, "Báo cáo học phần cần chọn một học phần.");
        if (request.Kind == "plo" && request.ProgramCourseId is not null)
            throw new ReportException(400, "Báo cáo PLO sử dụng toàn bộ phiên bản chương trình.");
    }

    public static ReportDocument Build(CreateReportRequest request, CurriculumReportSnapshot source,
        string userId, DateTimeOffset now)
    {
        Validate(request);
        if (source.Version.Id != request.VersionId || source.Version.ProgramId != source.Program.Id
            || source.Program.IsArchived || source.Version.ArchivedAt is not null)
            throw new ReportException(409, "Dữ liệu phiên bản không hợp lệ hoặc đã lưu trữ.");
        var courses = source.Courses.Where(c => c.ArchivedAt is null && c.Status != "archived"
            && c.ProgramVersionId == request.VersionId
            && (request.ProgramCourseId is null || c.Id == request.ProgramCourseId)).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).ToArray();
        if (request.ProgramCourseId is not null && courses.Length == 0)
            throw new ReportException(404, "Học phần không tồn tại trong phiên bản đang chọn.");
        var courseIds = courses.Select(c => c.Id).ToHashSet();
        var plos = source.Plos.Where(p => p.ProgramVersionId == request.VersionId).OrderBy(p => p.Code, StringComparer.Ordinal).ThenBy(p => p.Id).ToArray();
        var ploIds = plos.Select(p => p.Id).ToHashSet();
        var clos = source.Clos.Where(c => courseIds.Contains(c.ProgramCourseId) && c.Status != "archived")
            .OrderBy(c => Array.FindIndex(courses, x => x.Id == c.ProgramCourseId)).ThenBy(c => c.Code, StringComparer.Ordinal).ThenBy(c => c.Id).ToArray();
        var courseMappings = source.CourseMappings.Where(m => courseIds.Contains(m.ProgramCourseId)
            && m.ProgramVersionId == request.VersionId && ploIds.Contains(m.PloId)).OrderBy(m => m.Id).ToArray();
        if (courseMappings.Any(m => m.WeightCode is not ("X" or "Y") || m.ProgressionCode is not ("I" or "R" or "E")))
            throw new ReportException(409, "Dữ liệu mapping chứa mã ngoài X/Y hoặc I/R/E.");
        var mappingsById = courseMappings.ToDictionary(m => m.Id);
        var closById = clos.ToDictionary(c => c.Id);
        var links = source.CloMappings.Where(m => closById.TryGetValue(m.CloId, out var c)
            && mappingsById.TryGetValue(m.CoursePloId, out var cp) && c.ProgramCourseId == cp.ProgramCourseId
            && m.PloId == cp.PloId).DistinctBy(m => (m.CloId, m.CoursePloId))
            .OrderBy(m => m.CloId).ThenBy(m => m.CoursePloId).ToArray();
        var snapshot = source with { Courses = courses, Plos = plos, Clos = clos, CourseMappings = courseMappings, CloMappings = links };
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(snapshot, JsonOptions)))).ToLowerInvariant();
        var mappedClos = links.Select(l => l.CloId).ToHashSet();
        var supportedPlos = links.Select(l => l.PloId).ToHashSet();
        var linkedCourses = links.Select(l => closById[l.CloId].ProgramCourseId).Distinct().Count();
        var completeCourses = courses.Count(c => clos.Any(cl => cl.ProgramCourseId == c.Id)
            && clos.Where(cl => cl.ProgramCourseId == c.Id).All(cl => mappedClos.Contains(cl.Id)));
        var metrics = new[] {
            Metric("CLO có mapping", mappedClos.Count, clos.Length),
            Metric("PLO được CLO hỗ trợ trong phạm vi", supportedPlos.Count, plos.Length),
            Metric("Học phần có mapping qua CLO", linkedCourses, courses.Length),
            Metric("Học phần mapping đầy đủ CLO", completeCourses, courses.Length)
        };
        var findings = new List<string>();
        if (courses.Length == 0) findings.Add("Phiên bản chưa có học phần trong phạm vi báo cáo.");
        if (plos.Length == 0) findings.Add("Phiên bản chưa có PLO.");
        foreach (var c in courses)
        {
            if (!clos.Any(cl => cl.ProgramCourseId == c.Id)) findings.Add($"{CourseLabel(c)}: chưa có CLO.");
            if (string.IsNullOrWhiteSpace(c.InstitutionalCode)) findings.Add($"{c.Name}: chưa có mã học phần.");
        }
        foreach (var cl in clos.Where(c => !mappedClos.Contains(c.Id)))
            findings.Add($"{CourseLabel(courses.Single(c => c.Id == cl.ProgramCourseId))} / {cl.Code}: chưa liên kết PLO.");
        foreach (var p in plos.Where(p => !supportedPlos.Contains(p.Id))) findings.Add($"{p.Code}: chưa được CLO hỗ trợ trong phạm vi báo cáo.");
        foreach (var cp in courseMappings.Where(cp => !links.Any(l => l.CoursePloId == cp.Id)))
            findings.Add($"{CourseLabel(courses.Single(c => c.Id == cp.ProgramCourseId))} → {cp.PloCode}: có liên kết học phần–PLO nhưng chưa có CLO hỗ trợ.");
        if (findings.Count == 0) findings.Add("Không phát hiện thiếu CLO hoặc mapping theo các quy tắc cấu trúc đã kiểm tra. Chất lượng nội dung cần được chuyên gia xem xét.");

        var tables = new List<ReportTable>();
        tables.Add(new("Phạm vi học phần", ["Học phần", "Tín chỉ", "Học kỳ", "Số CLO"], courses.Select(c => new[] {
            CourseLabel(c), c.Credits.ToString(CultureInfo.InvariantCulture), c.Semester ?? "Chưa có", clos.Count(cl => cl.ProgramCourseId == c.Id).ToString(CultureInfo.InvariantCulture)
        }).ToArray()));
        tables.Add(new("Chuẩn đầu ra chương trình", ["PLO", "Nội dung", "Mức nguồn", "Nguồn dữ liệu"],
            plos.Select(p => new[] { p.Code, p.Statement, p.LevelCode ?? "Chưa có", p.Provenance }).ToArray()));
        if (request.Kind is "course" or "clo")
        {
            tables.Add(new("CLO và liên kết PLO", ["Học phần / CLO", "Nội dung CLO", "Mức nguồn", "PLO liên kết", "Nguồn dữ liệu"],
                clos.Select(cl => new[] { $"{CourseLabel(courses.Single(c => c.Id == cl.ProgramCourseId))} / {cl.Code}", cl.Statement,
                    cl.LevelCode ?? "Chưa có", Join(links.Where(l => l.CloId == cl.Id).Select(l => l.PloCode)), cl.Provenance }).ToArray()));
            foreach (var batch in plos.Chunk(6))
                tables.Add(new("Ma trận CLO–PLO (Có = có liên kết)", ["Học phần / CLO", ..batch.Select(p => p.Code)],
                    clos.Select(cl => new[] { $"{CourseLabel(courses.Single(c => c.Id == cl.ProgramCourseId))} / {cl.Code}" }
                        .Concat(batch.Select(p => links.Any(l => l.CloId == cl.Id && l.PloId == p.Id) ? "Có" : "—")).ToArray()).ToArray()));
        }
        tables.Add(new("Liên kết học phần–PLO", ["Học phần", "PLO", "Trọng số", "Tiến triển", "CLO hỗ trợ", "Nguồn dữ liệu"],
            courseMappings.Select(cp => new[] { CourseLabel(courses.Single(c => c.Id == cp.ProgramCourseId)), cp.PloCode, cp.WeightCode, cp.ProgressionCode,
                Join(links.Where(l => l.CoursePloId == cp.Id).Select(l => l.CloCode)), cp.Provenance }).ToArray()));
        if (request.Kind == "plo")
        {
            tables.Add(new("Tổng hợp PLO", ["PLO", "Học phần liên kết", "Số CLO hỗ trợ", "Số liên kết CLO–PLO", "Điểm phân bố"],
                plos.Select(p => new[] { p.Code, courseMappings.Where(cp => cp.PloId == p.Id).Select(cp => cp.ProgramCourseId).Distinct().Count().ToString(CultureInfo.InvariantCulture),
                    links.Where(l => l.PloId == p.Id).Select(l => l.CloId).Distinct().Count().ToString(CultureInfo.InvariantCulture),
                    links.Count(l => l.PloId == p.Id).ToString(CultureInfo.InvariantCulture),
                    courseMappings.Where(cp => cp.PloId == p.Id).Sum(cp => courses.Single(c => c.Id == cp.ProgramCourseId).Credits * (cp.WeightCode == "X" ? 2 : 1)).ToString(CultureInfo.InvariantCulture) }).ToArray()));
            foreach (var batch in plos.Chunk(6))
                tables.Add(new("Ma trận học phần–PLO (trọng số / tiến triển)", ["Học phần", ..batch.Select(p => p.Code)],
                    courses.Select(c => new[] { CourseLabel(c) }.Concat(batch.Select(p => {
                        var cp = courseMappings.SingleOrDefault(m => m.ProgramCourseId == c.Id && m.PloId == p.Id);
                        return cp is null ? "—" : $"{cp.WeightCode}/{cp.ProgressionCode}";
                    })).ToArray()).ToArray()));
        }
        string[] notes = [
            "Báo cáo cấu trúc và độ bao phủ thiết kế; không phải tỷ lệ sinh viên đạt chuẩn, GPA hoặc kết luận kiểm định.",
            "X=2, Y=1 là trọng số học phần–PLO; I/R/E là mức tiến triển. Không có mức đóng góp riêng trên từng cặp CLO–PLO.",
            "Điểm phân bố = tổng tín chỉ × trọng số X/Y; không nhân I/R/E. Phần trăm = tử số / mẫu số × 100; mẫu số 0 được ghi chưa có dữ liệu.",
            "Nguồn: Curriculum, chụp nhất quán theo phiên bản; chỉ gồm học phần/CLO đang hoạt động. Giữ provenance để phân biệt dữ liệu nguồn và dữ liệu sinh/gợi ý.",
            "Minh chứng: chưa tích hợp module SV4; báo cáo chưa kiểm tra nội dung hoặc sự tồn tại của minh chứng.",
            source.Version.Status == "draft" ? "Phiên bản CTĐT đang là bản nháp. Báo cáo chưa được phê duyệt." : "Trạng thái CTĐT không thay thế phê duyệt học thuật. Báo cáo chưa được phê duyệt."
        ];
        var title = request.Kind switch { "course" => "Báo cáo thiết kế học phần", "clo" => "Báo cáo CLO và mapping", _ => "Báo cáo PLO và độ bao phủ" };
        return new(Guid.NewGuid(), request.Kind, title, source.Program.Id, source.Version.Id, request.ProgramCourseId,
            source.Program.Code, source.Program.Name, source.Version.VersionCode, source.Version.Status, now, userId,
            RuleVersion, hash, metrics, findings.ToArray(), notes, tables.ToArray(), snapshot);
    }

    private static ReportMetric Metric(string label, int numerator, int denominator) =>
        new(label, numerator, denominator, denominator == 0 ? null : Math.Round(100m * numerator / denominator, 2));
    private static string CourseLabel(ProgramCourseResponse c) => $"{c.InstitutionalCode ?? $"HP#{c.Id}"} — {c.Name}";
    private static string Join(IEnumerable<string> values) => string.Join(", ", values.Distinct().Order(StringComparer.Ordinal)) is { Length: > 0 } s ? s : "Chưa có";
}
