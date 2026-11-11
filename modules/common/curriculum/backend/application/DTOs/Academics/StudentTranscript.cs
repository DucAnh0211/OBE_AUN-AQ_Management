using ObeAunQa.Modules.Curriculum.Domain;
namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record StudentTranscript(StudentInfo Student, IReadOnlyList<TranscriptSemester> Semesters, GpaSummary Cumulative,
    string? Classification, string RetakePolicy);
