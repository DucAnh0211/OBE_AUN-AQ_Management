namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeAttempt(long EnrollmentId, long StudentId, string StudentCode, string StudentName,
    string? Cohort, long OfferingId, long ProgramCourseId, string Period, string GradeStatus, int Revision,
    string? SpecialMark, decimal? FinalScore, decimal? GradePoint, bool CountsTowardGpa,
    bool Published, IReadOnlyDictionary<long, decimal?> Scores, bool? Passed = null);
