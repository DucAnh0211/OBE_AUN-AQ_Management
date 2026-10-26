namespace ObeAunQa.Modules.Curriculum.Application;

public interface ICloPloMappingRepository
{
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
}
