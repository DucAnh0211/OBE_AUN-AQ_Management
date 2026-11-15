namespace ObeAunQa.Modules.Curriculum.Application;

public interface IOutcomeReadService
{
    Task<OutcomeResult> ReadAsync(OutcomeScope scope, CancellationToken ct);
    Task<OutcomeSnapshotResponse> SnapshotAsync(long versionId, Guid id, CancellationToken ct);
}
