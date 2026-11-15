namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeSummary(string SchemaVersion, string SourceHash, string Status, OutcomeScope Scope,
    OutcomePolicy Policy, OutcomeCourse[] Courses, OutcomeIssue[] Issues, OutcomeAggregate[] Clos, OutcomeAggregate[] Plos);
