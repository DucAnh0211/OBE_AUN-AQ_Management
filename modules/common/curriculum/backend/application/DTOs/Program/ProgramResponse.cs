namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record ProgramResponse(
    long Id,
    long UniversityId,
    string UniversityCode,
    string UniversityName,
    string Code,
    string Name,
    bool IsArchived,
    long VersionCount,
    long? CurrentVersionId,
    string? CurrentVersionCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);
