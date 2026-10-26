namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record ProgramVersionResponse(
    long Id,
    long ProgramId,
    string VersionCode,
    string Status,
    bool IsCurrent,
    long? SourceVersionId,
    long CourseCount,
    DateTime? PublishedAt,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
