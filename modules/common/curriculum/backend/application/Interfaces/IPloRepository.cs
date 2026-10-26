namespace ObeAunQa.Modules.Curriculum.Application;

public interface IPloRepository
{
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
}
