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

    Task<IReadOnlyList<PloResponse>> GetPlosAsync(
        long versionId,
        CancellationToken cancellationToken);

    Task<PloResponse?> GetPloAsync(
        long versionId,
        long ploId,
        CancellationToken cancellationToken);

    Task<PloResponse> CreatePloAsync(
        long versionId,
        NormalizedLearningOutcome plo,
        CancellationToken cancellationToken);

    Task<PloResponse?> UpdatePloAsync(
        long versionId,
        long ploId,
        NormalizedLearningOutcome plo,
        CancellationToken cancellationToken);

    Task<bool> DeletePloAsync(
        long versionId,
        long ploId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CloResponse>> GetClosAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken);

    Task<CloResponse?> GetCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken);

    Task<CloResponse> CreateCloAsync(
        long versionId,
        long programCourseId,
        NormalizedLearningOutcome clo,
        CancellationToken cancellationToken);

    Task<CloResponse?> UpdateCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        NormalizedLearningOutcome clo,
        CancellationToken cancellationToken);

    Task<bool> DeleteCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CoursePloMappingResponse>> GetCoursePloMappingsAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken);

    Task<CoursePloMappingResponse> CreateCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        NormalizedCoursePloMapping mapping,
        CancellationToken cancellationToken);

    Task<CoursePloMappingResponse?> UpdateCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        NormalizedCoursePloMappingUpdate mapping,
        CancellationToken cancellationToken);

    Task<bool> DeleteCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CloPloMappingResponse>> GetCloPloMappingsAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken);

    Task<CloPloMappingResponse> CreateCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        long coursePloId,
        CancellationToken cancellationToken);

    Task<bool> DeleteCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        long coursePloId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CoursePloCreditCheckResponse>> GetPloCreditChecksAsync(
        long versionId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PloBalanceItemResponse>> GetPloBalanceAsync(
        long versionId,
        CancellationToken cancellationToken);
}
