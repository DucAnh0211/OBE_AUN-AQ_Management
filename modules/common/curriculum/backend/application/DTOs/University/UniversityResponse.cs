namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record UniversityResponse(
    long Id,
    string Code,
    string Name,
    bool IsArchived,
    long ProgramCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
