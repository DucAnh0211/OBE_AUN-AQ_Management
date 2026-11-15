using ObeAunQa.Modules.Curriculum.Application;
namespace ObeAunQa.Modules.Reporting.Application;

/// <summary>Public integration boundary: reporting consumes the same authorized immutable OBE result as the dashboard.</summary>
public sealed class OutcomeReportSource(IOutcomeReadService outcomes)
{
    public Task<OutcomeSnapshotResponse> ReadAsync(long versionId, Guid snapshotId, CancellationToken ct) => outcomes.SnapshotAsync(versionId,snapshotId,ct);
}
