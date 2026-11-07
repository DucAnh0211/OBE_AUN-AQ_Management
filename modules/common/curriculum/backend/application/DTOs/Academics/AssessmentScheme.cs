namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AssessmentScheme(long VersionId, int Revision, bool Locked, IReadOnlyList<AssessmentComponentInput> Components);
