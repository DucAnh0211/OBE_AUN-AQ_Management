namespace ObeAunQa.Modules.Curriculum.Application;

public interface ICloRepository
{
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
}
