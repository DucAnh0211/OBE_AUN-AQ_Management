namespace ObeAunQa.Modules.Curriculum.Application;

public interface ICurriculumRepository
{
    Task<PagedResult<ProgramResponse>> GetProgramsAsync(
        int page,
        int pageSize,
        string? search,
        bool includeArchived,
        CancellationToken cancellationToken);

    Task<ProgramResponse?> GetProgramAsync(long id, CancellationToken cancellationToken);

    Task<ProgramResponse> CreateProgramAsync(
        string code,
        string name,
        CancellationToken cancellationToken);

    Task<ProgramResponse?> UpdateProgramAsync(
        long id,
        string name,
        CancellationToken cancellationToken);

    Task<bool> ArchiveProgramAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProgramVersionResponse>> GetProgramVersionsAsync(
        long programId,
        bool includeArchived,
        CancellationToken cancellationToken);

    Task<ProgramVersionResponse?> GetProgramVersionAsync(
        long id,
        CancellationToken cancellationToken);

    Task<ProgramVersionResponse> CreateProgramVersionAsync(
        long programId,
        string versionCode,
        long? sourceVersionId,
        CancellationToken cancellationToken);

    Task<ProgramVersionResponse?> UpdateProgramVersionAsync(
        long id,
        string versionCode,
        CancellationToken cancellationToken);

    Task<ProgramVersionResponse> PublishProgramVersionAsync(
        long id,
        CancellationToken cancellationToken);

    Task<bool> ArchiveProgramVersionAsync(long id, CancellationToken cancellationToken);

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
