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

public sealed record CreatePloRequest(
    string? Code,
    string? Statement,
    string? LevelCode);

public sealed record UpdatePloRequest(
    string? Code,
    string? Statement,
    string? LevelCode);

public sealed record CreateCloRequest(
    string? Code,
    string? Statement,
    string? LevelCode);

public sealed record UpdateCloRequest(
    string? Code,
    string? Statement,
    string? LevelCode);

public sealed record CreateCoursePloMappingRequest(
    long PloId,
    string? WeightCode,
    string? ProgressionCode,
    string? Fit,
    string? FitReason);

public sealed record UpdateCoursePloMappingRequest(
    string? WeightCode,
    string? ProgressionCode,
    string? Fit,
    string? FitReason);

public sealed record CreateCloPloMappingRequest(long CoursePloId);

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

public sealed record PloResponse(
    long Id,
    long ProgramVersionId,
    string Code,
    string Statement,
    string? LevelCode,
    string Provenance);

public sealed record CloResponse(
    long Id,
    long ProgramCourseId,
    string Code,
    string Statement,
    string? LevelCode,
    string Provenance,
    string Status);

public sealed record CoursePloMappingResponse(
    long Id,
    long ProgramVersionId,
    long ProgramCourseId,
    long PloId,
    string PloCode,
    string WeightCode,
    string ProgressionCode,
    string Provenance,
    string? Fit,
    string? FitReason);

public sealed record CloPloMappingResponse(
    long CloId,
    string CloCode,
    long CoursePloId,
    long PloId,
    string PloCode);

public sealed record CoursePloCreditCheckResponse(
    long ProgramCourseId,
    string? InstitutionalCode,
    string CourseName,
    int Credits,
    int RequiredPloCount,
    int ActualPloCount,
    bool IsValid);

public sealed record PloCreditCheckResponse(
    long ProgramVersionId,
    int MaximumRequiredPloCount,
    bool IsValid,
    int ViolationCount,
    IReadOnlyList<CoursePloCreditCheckResponse> Courses);

public sealed record PloBalanceItemResponse(
    string PloCode,
    decimal Score,
    decimal MeanScore,
    decimal? Deviation,
    bool ExceedsTwentyPercent);

public sealed record PloBalanceResponse(
    long ProgramVersionId,
    decimal AllowedDeviation,
    bool IsBalanced,
    IReadOnlyList<PloBalanceItemResponse> Items);

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
