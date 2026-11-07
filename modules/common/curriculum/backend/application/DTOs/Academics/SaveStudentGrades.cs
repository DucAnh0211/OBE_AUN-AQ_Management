namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record SaveStudentGrades(int Revision, int SchemeRevision, IReadOnlyDictionary<string, decimal?> Scores);
