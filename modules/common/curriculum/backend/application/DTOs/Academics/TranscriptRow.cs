namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record TranscriptRow(string Period, string CourseCode, string CourseName, int Credits, bool CountsTowardGpa, string OfferingCode,
    decimal? FinalScore, string? LetterGrade, decimal? GradePoint, bool? Passed, string AttemptKind, bool CountedInCumulative);
