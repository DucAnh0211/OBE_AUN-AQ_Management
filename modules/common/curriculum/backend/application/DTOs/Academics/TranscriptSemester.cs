using ObeAunQa.Modules.Curriculum.Domain;
namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record TranscriptSemester(string Period, IReadOnlyList<TranscriptRow> Rows, GpaSummary Summary);
