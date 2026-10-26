namespace ObeAunQa.Modules.Curriculum.Application;

public interface ICurriculumAnalysisRepository
{
    Task<IReadOnlyList<CoursePloCreditCheckResponse>> GetPloCreditChecksAsync(
        long versionId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PloBalanceItemResponse>> GetPloBalanceAsync(
        long versionId,
        CancellationToken cancellationToken);
}
