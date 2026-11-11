namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AssessmentCoverageItem(long CourseId, string CourseCode, string CourseName, int ComponentCount, IReadOnlyList<string> UnmappedComponents, IReadOnlyList<string> UnassessedClos);
