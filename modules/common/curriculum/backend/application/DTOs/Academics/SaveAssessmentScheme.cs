namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record SaveAssessmentScheme(int Revision, IReadOnlyList<AssessmentComponentInput> Components);
