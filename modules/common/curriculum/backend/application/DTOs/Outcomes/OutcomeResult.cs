namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeResult(string SchemaVersion, OutcomeInput Input, string SourceHash, string Status,
    OutcomeIssue[] Issues, StudentOutcome[] CloStudents, OutcomeAggregate[] Clos,
    StudentOutcome[] PloStudents, OutcomeAggregate[] Plos, OutcomeGpa[]? Gpas = null);
