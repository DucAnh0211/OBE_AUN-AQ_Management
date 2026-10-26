namespace ObeAunQa.Modules.Reporting.Application;

public interface IReportRepository
{
    Task SaveAsync(ReportDocument document, CancellationToken ct);
    Task<ReportDocument?> GetAsync(Guid id, long[] programIds, CancellationToken ct);
    Task<ReportPage> ListAsync(long[] programIds, int page, int pageSize, CancellationToken ct);
    Task<bool> RevokeAsync(Guid id, long[] programIds, string actor, CancellationToken ct);
    Task AuditAsync(Guid id, string actor, string action, CancellationToken ct);
}
