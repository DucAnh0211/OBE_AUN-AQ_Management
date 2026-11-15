using ObeAunQa.Modules.Curriculum.Domain;
namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeGpa(long StudentId, string StudentCode, string Period, GpaSummary Semester, GpaSummary Cumulative, string ScopeNote);
