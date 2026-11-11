namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record EnrollmentInfo(long Id, string StudentCode, string FullName, string AcademicPeriod, int Revision,
    int GradedComponents, decimal? TotalScore, string? LetterGrade = null, decimal? GradePoint = null, bool? Passed = null,
    string? SpecialMark = null, string AttemptKind = "first", string? HomeClassCode = null, bool Published = false);
