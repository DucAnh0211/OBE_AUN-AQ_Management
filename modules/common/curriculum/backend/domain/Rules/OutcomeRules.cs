using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Curriculum.Domain;

public static partial class OutcomeRules
{
    private static OutcomeAttempt ChooseAttempt(IEnumerable<OutcomeAttempt> source, OutcomeInput input)
    {
        var all=source.ToArray();
        var quantitative=all.Where(a=>a.SpecialMark==null && (input.Scope.Preview||a.Published) && (a.FinalScore!=null||input.Scope.Preview));
        var ordered=input.Policy.RetakePolicy=="highest" ? quantitative.OrderByDescending(a=>a.FinalScore).ThenByDescending(a=>PeriodOrder(a.Period)).ThenByDescending(a=>a.OfferingId)
            : quantitative.OrderByDescending(a=>PeriodOrder(a.Period)).ThenByDescending(a=>a.OfferingId);
        return ordered.FirstOrDefault()??all.OrderByDescending(a=>PeriodOrder(a.Period)).ThenByDescending(a=>a.OfferingId).First();
    }
    public static void Validate(OutcomePolicy p)
    {
        if (p.CloThreshold is < 0 or > 10 || p.PloThreshold is < 0 or > 10 || p.GroupTargetRate is < 0 or > 100
            || p.RetakePolicy is not ("highest" or "latest") || p.FormulaVersion != "obe/1")
            AcademicValidator.Invalid("policy", "Ngưỡng hoặc công thức OBE không hợp lệ.");
    }
    public static int PeriodOrder(string period)
    {
        var match = PeriodPattern().Match(period);
        if (!match.Success) return -1;
        return int.Parse(match.Groups[1].Value) * 10 + (match.Groups[2].Value.ToUpperInvariant() == "HE" ? 3 : int.Parse(match.Groups[2].Value));
    }
    public static string Hash(OutcomeInput input) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
        JsonSerializer.Serialize(input)))).ToLowerInvariant();
    public static OutcomeAggregate Aggregate(long id, string code, IEnumerable<StudentOutcome> source, decimal target)
    {
        var rows = source.ToArray(); var excluded = rows.Count(r => r.Status == "excluded");
        var eligible = rows.Length - excluded; var values = rows.Where(r => r.Score != null && r.Status != "excluded").ToArray();
        var achieved = values.Count(r => r.Status == "achieved");
        decimal? rate = values.Length == 0 ? null : 100m * achieved / values.Length;
        decimal? coverage = eligible == 0 ? null : 100m * values.Length / eligible;
        return new(id, code, eligible, values.Length, achieved, eligible - values.Length, excluded, rate, coverage,
            values.Length == 0 ? null : values.Average(r => r.Score!.Value), eligible == 0 ? "no_data" : coverage < 100 ? "partial" : rate >= target ? "achieved" : "not_achieved");
    }
    public static OutcomeResult Calculate(OutcomeInput input)
    {
        Validate(input.Policy); var issues = new List<OutcomeIssue>(); var clos = new List<StudentOutcome>();
        void Issue(string code, long id, string message) => issues.Add(new(code, id, message));
        foreach (var course in input.Courses)
        {
            var invalid = course.Assessments.Length == 0 || course.Assessments.Sum(a => a.Weight) != 100
                || course.Assessments.Any(a => a.Weight <= 0 || a.MaxScore <= 0 || a.Clos.Values.Sum() != 100 || a.Clos.Values.Any(q => q <= 0 || q > 100)
                    || a.Clos.Keys.Any(id => course.Clos.All(c => c.Id != id)));
            if (invalid) Issue("assessment_mapping", course.Id, $"{course.Code}: cơ cấu điểm hoặc tỷ trọng Assessment–CLO chưa hợp lệ (mỗi đầu điểm phải map tổng 100%).");
            if (course.Clos.Length == 0) Issue("no_clos", course.Id, $"{course.Code}: chưa có CLO.");
            foreach (var clo in course.Clos)
            {
                var assessments = course.Assessments.Where(a => a.Clos.ContainsKey(clo.Id)).ToArray();
                if (assessments.Length == 0) Issue("unassessed_clo", clo.Id, $"{course.Code}/{clo.Code}: chưa được đánh giá.");
                if (!course.Links.Any(l => l.CloIds.Contains(clo.Id))) Issue("unmapped_clo", clo.Id, $"{clo.Code}: chưa liên kết PLO.");
                foreach (var a in input.Attempts.Where(a => a.ProgramCourseId == course.Id))
                {
                    var contributions = assessments.Select(s => new OutcomeContribution(s.Id, a.Scores.GetValueOrDefault(s.Id), s.MaxScore, s.Weight / 100 * s.Clos[clo.Id] / 100)).ToArray();
                    var excluded = a.SpecialMark is "R" or "P";
                    var missing = invalid || contributions.Length == 0 || a.SpecialMark == "I" || (!input.Scope.Preview && !a.Published)
                        || contributions.Any(c => c.Score is null || c.Score < 0 || c.Score > c.MaxScore);
                    decimal? score = excluded || missing ? null : contributions.Sum(c => 10 * c.Score!.Value / c.MaxScore * c.EffectiveWeight) / contributions.Sum(c => c.EffectiveWeight);
                    clos.Add(new(a.StudentId, a.StudentCode, a.StudentName, a.OfferingId, course.Id, clo.Id, clo.Code, score, input.Policy.CloThreshold,
                        excluded ? "excluded" : score is null ? "insufficient_data" : score >= input.Policy.CloThreshold ? "achieved" : "not_achieved",
                        excluded ? "Điểm miễn/đạt không có đánh giá định lượng." : missing ? "Thiếu điểm, cấu hình hoặc chưa công bố." : null, contributions, [a.EnrollmentId]));
                }
            }
        }
        foreach (var a in input.Attempts.Where(a => PeriodOrder(a.Period) < 0)) Issue("invalid_period", a.OfferingId, "Học kỳ phải có dạng YYYY-YYYY-HK1/HK2/HKHE.");
        foreach (var duplicate in input.Attempts.GroupBy(a=>new { a.StudentId, a.ProgramCourseId, a.Period }).Where(g=>g.Count()>1))
            Issue("duplicate_enrollment",duplicate.Key.StudentId,"Sinh viên có nhiều ghi danh cùng học phần trong một kỳ; cần rà soát trước khi kết luận.");
        var plos = new List<StudentOutcome>();
        foreach (var plo in input.Plos)
        {
            var courses = input.Courses.Where(c => c.Links.Any(l => l.PloId == plo.Id)).ToArray();
            if (courses.Length == 0) Issue("unsupported_plo", plo.Id, $"{plo.Code}: chưa có học phần hỗ trợ trong phạm vi.");
            foreach (var student in input.Attempts.GroupBy(a => a.StudentId))
            {
                var all = student.ToArray(); var selected = all.GroupBy(a => input.Courses.Single(c => c.Id == a.ProgramCourseId).CourseId)
                    .Select(g => ChooseAttempt(g,input)).ToArray();
                var scores = new List<(decimal Score, decimal Weight)>(); var ids = new List<long>(); var excluded = 0;
                foreach (var course in courses)
                {
                    var attempt = selected.SingleOrDefault(a => a.ProgramCourseId == course.Id); if (attempt is null) continue;
                    ids.Add(attempt.EnrollmentId);
                    if (attempt.SpecialMark is "R" or "P") { excluded++; continue; }
                    var link = course.Links.Single(l => l.PloId == plo.Id);
                    var children = clos.Where(c => c.StudentId == student.Key && c.ProgramCourseId == course.Id && c.OfferingId == attempt.OfferingId && link.CloIds.Contains(c.OutcomeId)).ToArray();
                    if (children.Length == 0 || children.Length != link.CloIds.Length || children.Any(c => c.Score == null) || course.Credits <= 0 || link.WeightCode is not ("X" or "Y")) continue;
                    scores.Add((children.Average(c => c.Score!.Value), course.Credits * (link.WeightCode == "X" ? 2m : 1m)));
                }
                var expected = courses.Length - excluded;
                // A missing required course makes this scoped PLO incomplete; never inflate coverage by dropping it.
                decimal? score = expected == 0 || scores.Count != expected ? null : scores.Sum(s => s.Score * s.Weight) / scores.Sum(s => s.Weight);
                var first = all[0]; plos.Add(new(student.Key, first.StudentCode, first.StudentName, null, null, plo.Id, plo.Code, score, input.Policy.PloThreshold,
                    expected == 0 ? "excluded" : score is null ? "insufficient_data" : score >= input.Policy.PloThreshold ? "achieved" : "not_achieved",
                    score == null ? "Kết quả trong phạm vi; chưa đủ học phần/CLO hoặc không có dữ liệu định lượng." : null, [], ids.ToArray(), expected, scores.Count));
            }
        }
        var cloAggregates = input.Courses.SelectMany(c => c.Clos).Select(c => Aggregate(c.Id, c.Code, clos.Where(r => r.OutcomeId == c.Id)
            .GroupBy(r=>r.StudentId).Select(g=> {
                var ids=g.SelectMany(r=>r.EnrollmentIds).ToHashSet();var chosen=ChooseAttempt(input.Attempts.Where(a=>ids.Contains(a.EnrollmentId)),input);
                return g.Single(r=>r.EnrollmentIds.Contains(chosen.EnrollmentId));
            }), input.Policy.GroupTargetRate)).ToArray();
        var ploAggregates = input.Plos.Select(p => Aggregate(p.Id, p.Code, plos.Where(r => r.OutcomeId == p.Id), input.Policy.GroupTargetRate)).ToArray();
        var status = input.Scope.Preview ? "preview" : input.Attempts.Length == 0 ? "no_data" : issues.Count > 0 || cloAggregates.Any(c => c.MissingCount>0) || ploAggregates.Any(p => p.MissingCount>0) ? "partial" : cloAggregates.All(c=>c.EvaluatedCount==0) ? "no_data" : "official";
        var gpas = input.Attempts.Where(a => a.Published).GroupBy(a => a.StudentId).SelectMany(student => {
            TranscriptAttempt Attempt(OutcomeAttempt a) => new(input.Courses.Single(c=>c.Id==a.ProgramCourseId).CourseId,a.Period,
                input.Courses.Single(c=>c.Id==a.ProgramCourseId).Credits,a.CountsTowardGpa,a.GradePoint,a.Passed);
            return student.GroupBy(a=>a.Period).Select(period=>new OutcomeGpa(student.Key,student.First().StudentCode,period.Key,
                GpaRules.Semester(period.Select(Attempt),input.GpaDecimals),GpaRules.Cumulative(student.Select(Attempt),input.AcademicRetakePolicy,input.GpaDecimals),
                "GPA chỉ trong phạm vi dữ liệu đã chọn; bảng điểm sinh viên cung cấp GPA toàn bộ."));
        }).ToArray();
        return new("obe/1", input, Hash(input), status, issues.Distinct().ToArray(), clos.ToArray(), cloAggregates, plos.ToArray(), ploAggregates, gpas);
    }
    [GeneratedRegex(@"^(\d{4})-\d{4}-HK(1|2|HE)$", RegexOptions.IgnoreCase)]
    private static partial Regex PeriodPattern();
}
