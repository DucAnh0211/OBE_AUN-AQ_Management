namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record ProgramCourseResponse(
    long Id,
    long ProgramVersionId,
    long CourseId,
    string? InstitutionalCode,
    string Name,
    int Credits,
    string? Semester,
    int DisplayOrder,
    string Status,
    int? SourceRow,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
