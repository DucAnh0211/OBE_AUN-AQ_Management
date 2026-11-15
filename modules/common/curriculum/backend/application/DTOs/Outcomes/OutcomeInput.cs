namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeInput(long UniversityId, long ProgramId, OutcomeScope Scope, OutcomePolicy Policy,
    OutcomeCourse[] Courses, OutcomeDefinition[] Plos, OutcomeAttempt[] Attempts, string AcademicRetakePolicy = "highest", int GpaDecimals = 2);
