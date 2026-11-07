namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record StudentGradebook(EnrollmentInfo Enrollment, AssessmentScheme Scheme, IReadOnlyDictionary<string, decimal?> Scores,
    IReadOnlyList<GradeChangeInfo> History);
