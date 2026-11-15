namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeCourse(long Id, long CourseId, string Code, string Name, int Credits,
    OutcomeDefinition[] Clos, OutcomeAssessment[] Assessments, OutcomeLink[] Links);
