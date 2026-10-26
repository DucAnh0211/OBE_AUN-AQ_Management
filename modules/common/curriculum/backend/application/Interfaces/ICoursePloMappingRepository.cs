namespace ObeAunQa.Modules.Curriculum.Application;

public interface ICoursePloMappingRepository
{
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
}
