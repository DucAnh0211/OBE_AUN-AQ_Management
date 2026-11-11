using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
namespace ObeAunQa.Modules.Curriculum.Tests;

/// <summary>Quy tắc theo Thông tư 08/2021/TT-BGDĐT: điểm học phần, điểm chữ, thang 4, GPA, học lại.</summary>
public sealed class AcademicRecordsTests
{
    [Theory]
    [InlineData(8.5, "A", 4)] [InlineData(8.4, "B", 3)] [InlineData(7.0, "B", 3)] [InlineData(6.9, "C", 2)]
    [InlineData(5.5, "C", 2)] [InlineData(4.0, "D", 1)] [InlineData(3.9, "F", 0)] [InlineData(0, "F", 0)] [InlineData(10, "A", 4)]
    public void DefaultBandsFollowCircular08(double score, string letter, int point)
    {
        var band = GradeRules.Band((decimal)score, GradeRules.DefaultBands)!;
        Assert.Equal(letter, band.Letter); Assert.Equal(point, band.Point4); Assert.Equal(letter != "F", band.IsPassing);
    }

    [Fact]
    public void CourseScoreIsRoundedToOneDecimalBeforeLetterConversion()
    {
        // 8.45 → 8.5 (A); làm tròn trước khi quy đổi điểm chữ.
        Assert.Equal(8.5m, GradeRules.Total([(8.45m, 10m, 100m)], 1));
        Assert.Equal("A", GradeRules.Outcome(GradeRules.Total([(8.45m, 10m, 100m)], 1), null, GradeRules.DefaultBands).Letter);
        Assert.Equal(8.45m, GradeRules.Total([(8.45m, 10m, 100m)], 2));
    }

    [Fact]
    public void SpecialMarksAreNotCountedAsScores()
    {
        Assert.Equal(new GradeOutcome(null, "I", null, null), GradeRules.Outcome(7m, "I", GradeRules.DefaultBands));
        Assert.Equal(new GradeOutcome(null, "R", null, true), GradeRules.Outcome(null, "R", GradeRules.DefaultBands));
        Assert.Equal(new GradeOutcome(null, null, null, null), GradeRules.Outcome(null, null, GradeRules.DefaultBands));
        Assert.Equal(false, GradeRules.Outcome(3.9m, null, GradeRules.DefaultBands).Passed);
    }

    [Fact]
    public void AttemptKindDistinguishesRetakeAndImprovement()
    {
        Assert.Equal("first", GradeRules.AttemptKind([]));
        Assert.Equal("retake", GradeRules.AttemptKind([false]));
        Assert.Equal("retake", GradeRules.AttemptKind([null]));
        Assert.Equal("improve", GradeRules.AttemptKind([false, true]));
    }

    [Fact]
    public void BandValidationRejectsGapsDuplicatesAndInversions()
    {
        Assert.Empty(GradeRules.BandErrors(GradeRules.DefaultBands));
        Assert.NotEmpty(GradeRules.BandErrors([new("A", 8.5m, 4, true), new("B", 7m, 3, true)]));
        Assert.NotEmpty(GradeRules.BandErrors([new("A", 5m, 1, true), new("F", 0m, 2, false)]));
        Assert.NotEmpty(GradeRules.BandErrors([new("A", 5m, 4, false), new("F", 0m, 0, true)]));
        Assert.NotEmpty(GradeRules.BandErrors([new("A", 5m, 4, true), new("A", 0m, 0, false)]));
    }

    private static TranscriptAttempt A(long course, string period, int credits, decimal? point, bool? passed, bool gpa = true) => new(course, period, credits, gpa, point, passed);

    [Fact]
    public void SemesterGpaWeightsByCreditsAndExcludesNonGpaCourses()
    {
        var gpa = GpaRules.Semester([A(1, "2025-2026-HK1", 3, 4, true), A(2, "2025-2026-HK1", 2, 2, true), A(3, "2025-2026-HK1", 1, null, true, gpa: false)]);
        Assert.Equal(3.2m, gpa.Gpa); Assert.Equal(5, gpa.GpaCredits); Assert.Equal(6, gpa.EarnedCredits);
        Assert.Null(GpaRules.Semester([A(3, "x", 1, null, true, gpa: false)]).Gpa);
    }

    [Fact]
    public void CumulativeGpaKeepsOneAttemptPerCourseByPolicy()
    {
        var rows = new[] { A(1, "2024-2025-HK1", 3, 0, false), A(1, "2025-2026-HK1", 3, 2, true), A(1, "2025-2026-HK2", 3, 1, true), A(2, "2024-2025-HK1", 2, 4, true) };
        var highest = GpaRules.Cumulative(rows, AcademicCodes.Highest);
        Assert.Equal(decimal.Round((2m * 3 + 4m * 2) / 5, 2), highest.Gpa); Assert.Equal(5, highest.EarnedCredits);
        var latest = GpaRules.Cumulative(rows, AcademicCodes.Latest);
        Assert.Equal(decimal.Round((1m * 3 + 4m * 2) / 5, 2), latest.Gpa);
        Assert.Equal("2025-2026-HK1", GpaRules.Chosen(rows.Where(r => r.CourseId == 1), AcademicCodes.Highest)!.Period);
    }

    [Theory]
    [InlineData(3.6, "Xuất sắc")] [InlineData(3.59, "Giỏi")] [InlineData(3.2, "Giỏi")] [InlineData(2.5, "Khá")]
    [InlineData(2.0, "Trung bình")] [InlineData(1.0, "Yếu")] [InlineData(0.99, "Kém")]
    public void ClassificationUsesCircular08Bands(double gpa, string label) => Assert.Equal(label, GpaRules.Classification((decimal)gpa));

    private static SaveAssessmentScheme Scheme(params AssessmentComponentInput[] components) => new(0, components);

    [Fact]
    public void SchemeEnforcesUniversityPolicyAndCloCodes()
    {
        var ok = Scheme(new AssessmentComponentInput("QT", "Quá trình", 40, 10, "process", new Dictionary<string, decimal> { ["CLO1"] = 100 }), new("THI", "Thi cuối kỳ", 60, 10, "final"));
        AcademicValidator.Scheme(ok, 2, 50, ["CLO1"]);
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scheme(Scheme(new AssessmentComponentInput("THI", "Thi cuối kỳ", 100, 10, "final")), 2));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scheme(ok, 2, 70, ["CLO1"]));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scheme(ok, 2, null, ["CLO2"]));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scheme(Scheme(new AssessmentComponentInput("QT", "Quá trình", 100, 10, "other"))));
        AcademicValidator.Scheme(Scheme(new AssessmentComponentInput("THI", "Thi cuối kỳ", 100, 10, "final")), 1, 40);
    }

    [Fact]
    public void OfferingNeedsExactlyOneMainLecturerWhenAssigned()
    {
        Assert.Empty(AcademicValidator.Lecturers([]));
        AcademicValidator.Lecturers([new(1, "main"), new(2, "assistant")]);
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Lecturers([new(1, "assistant")]));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Lecturers([new(1, "main"), new(2, "main")]));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Lecturers([new(1, "main"), new(1, "practice")]));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Offering(new CreateCourseOffering(1, "2026-2027-HK1", Capacity: 0)));
        Assert.Equal("N02", AcademicValidator.Offering(new CreateCourseOffering(1, "2026-2027-HK1", "n02")).GroupName);
    }

    [Fact]
    public void PolicyAndOrganizationInputsAreNormalized()
    {
        var policy = AcademicValidator.Policy(new(1, 2, 50, "highest", 2, [new("a", 8.5m, 4, true), new("b+", 7.5m, 3.5m, true), new("f", 0, 0, false)]));
        Assert.Equal(new[] { "A", "B+", "F" }, policy.Bands.Select(b => b.Letter));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Policy(new(3, 2, null, "highest", 2, GradeRules.DefaultBands.Select(b => new GradeBandInput(b.Letter, b.MinScore, b.Point4, b.IsPassing)).ToArray())));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Policy(new(1, 2, null, "first", 2, [])));
        Assert.Equal("CNTT", AcademicValidator.OrgUnit(new("faculty", "cntt", "Khoa CNTT", null)).Code);
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.OrgUnit(new("department", "KHMT", "Bộ môn KHMT", null)));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Lecturer(new("GV01", null, "TS", null)));
        Assert.Equal("K66", AcademicValidator.HomeClass(new("cntt01-k66", "CNTT 01", "k66", null)).CohortCode);
        Assert.Null(AcademicValidator.SpecialMark(null)); Assert.Equal("P", AcademicValidator.SpecialMark(" p "));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.SpecialMark("X"));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Reason(" ", required: true));
    }
}
