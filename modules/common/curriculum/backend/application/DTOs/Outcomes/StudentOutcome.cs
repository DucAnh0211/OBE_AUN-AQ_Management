namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record StudentOutcome(long StudentId, string StudentCode, string StudentName, long? OfferingId,
    long? ProgramCourseId, long OutcomeId, string Code, decimal? Score, decimal Threshold, string Status,
    string? Reason, OutcomeContribution[] Contributions, long[] EnrollmentIds, int ExpectedCourses = 0, int EvaluatedCourses = 0);
