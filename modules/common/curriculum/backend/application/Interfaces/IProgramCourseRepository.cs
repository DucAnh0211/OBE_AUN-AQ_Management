namespace ObeAunQa.Modules.Curriculum.Application;

public interface IProgramCourseRepository
{
    Task<PagedResult<ProgramCourseResponse>> GetProgramCoursesAsync(
        long versionId,
        int page,
        int pageSize,
        string? search,
        string? semester,
        bool includeArchived,
        CancellationToken cancellationToken);

    Task<ProgramCourseResponse?> GetProgramCourseAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken);

    Task<ProgramCourseResponse> CreateProgramCourseAsync(
        long versionId,
        NormalizedCourse course,
        CancellationToken cancellationToken);

    Task<ProgramCourseResponse?> UpdateProgramCourseAsync(
        long versionId,
        long programCourseId,
        NormalizedCourse course,
        CancellationToken cancellationToken);

    Task<bool> ArchiveProgramCourseAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken);
}
