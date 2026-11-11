namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AssessmentScheme(long CourseId, int Revision, bool Locked, IReadOnlyList<AssessmentComponentInput> Components, IReadOnlyList<string>? CloCodes = null);
