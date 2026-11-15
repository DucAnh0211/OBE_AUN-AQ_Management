namespace ObeAunQa.Modules.Curriculum.Application;

public interface IOutcomeAnalyticsService : IOutcomeReadService
{
    Task<OutcomePolicy> PolicyAsync(long versionId, CancellationToken ct);
    Task<OutcomePolicy> SavePolicyAsync(long versionId, OutcomePolicy request, CancellationToken ct);
    Task<OutcomeSnapshotResponse> CreateSnapshotAsync(OutcomeScope scope, CancellationToken ct);
    Task<object> CompareAsync(OutcomeScope scope, string otherPeriod, CancellationToken ct);
    Task<OutcomeOptions> OptionsAsync(long versionId, CancellationToken ct);
    Task<object> AuditAsync(int page, CancellationToken ct);
    Task<OutcomeSnapshotResponse?> LatestAsync(OutcomeScope scope, CancellationToken ct);
    Task<OutcomeSummary> SummaryAsync(OutcomeScope scope, CancellationToken ct);
    Task<PagedResult<StudentOutcome>> StudentsAsync(OutcomeScope scope, int page, string? search, CancellationToken ct);
}
