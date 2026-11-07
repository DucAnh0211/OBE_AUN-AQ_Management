namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AcademicPeriodBatch(string Name, IReadOnlyList<AcademicSemesterOption> Semesters);
