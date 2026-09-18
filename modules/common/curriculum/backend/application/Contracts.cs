namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CreateProgramRequest(string? Code, string? Name);

public sealed record UpdateProgramRequest(string? Name);

public sealed record CreateProgramVersionRequest(string? VersionCode, long? SourceVersionId);

public sealed record UpdateProgramVersionRequest(string? VersionCode);

public sealed record CreateProgramCourseRequest(
    string? InstitutionalCode,
    string? Name,
    int Credits,
    string? Semester,
    int? DisplayOrder);

public sealed record UpdateProgramCourseRequest(
    string? InstitutionalCode,
    string? Name,
    int Credits,
    string? Semester,
    int DisplayOrder);

public sealed record ProgramResponse(
    long Id,
    string Code,
    string Name,
    bool IsArchived,
    long VersionCount,
    long? CurrentVersionId,
    string? CurrentVersionCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

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

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalItems,
    int TotalPages)
{
    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        long totalItems)
    {
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<T>(items, page, pageSize, totalItems, totalPages);
    }
}
