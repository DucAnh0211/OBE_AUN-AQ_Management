using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Reporting.Application;
using ObeAunQa.Modules.Reporting.Domain.Entities;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

internal sealed class EfReportRepository(ReportingDbContext db) : IReportRepository
{
    public async Task SaveAsync(ReportDocument document, CancellationToken ct)
    {
        db.Reports.Add(new Report { Id = document.Id, ProgramId = document.ProgramId, VersionId = document.VersionId, Kind = document.Kind,
            Title = document.Title, CreatedAt = document.CreatedAt.UtcDateTime, CreatedBy = document.CreatedBy, SnapshotHash = document.SnapshotHash,
            Document = JsonSerializer.Serialize(document, ReportBuilder.JsonOptions),
            AuditEvents = [new AuditEvent { ActorId = document.CreatedBy, Action = "created" }] });
        await db.SaveChangesAsync(ct); // The snapshot and audit are committed atomically by EF.
    }
    private IQueryable<Report> Visible(long[] programIds) => db.Reports.AsNoTracking().Where(r => programIds.Contains(r.ProgramId) && r.RevokedAt == null);
    public async Task<ReportDocument?> GetAsync(Guid id, long[] programIds, CancellationToken ct)
    {
        var json = await Visible(programIds).Where(r => r.Id == id).Select(r => r.Document).SingleOrDefaultAsync(ct);
        return json == null ? null : JsonSerializer.Deserialize<ReportDocument>(json, ReportBuilder.JsonOptions);
    }
    public async Task<ReportPage> ListAsync(long[] programIds, int page, int pageSize, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var query = Visible(programIds);
        var count = await query.LongCountAsync(ct);
        var rows = await query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(r => new ReportSummary(r.Id, r.Kind, r.Title, r.ProgramId, r.VersionId, r.CreatedAt, r.CreatedBy, r.SnapshotHash)).ToArrayAsync(ct);
        await tx.CommitAsync(ct);
        return new(rows, page, pageSize, count);
    }
    public async Task<bool> RevokeAsync(Guid id, long[] programIds, string actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var changed = await Visible(programIds).Where(r => r.Id == id).ExecuteUpdateAsync(s => s.SetProperty(r => r.RevokedAt, DateTime.UtcNow).SetProperty(r => r.RevokedBy, actor), ct);
        if (changed == 0) return false;
        await AuditAsync(id, actor, "revoked", ct);
        await tx.CommitAsync(ct);
        return true;
    }
    public async Task AuditAsync(Guid id, string actor, string action, CancellationToken ct)
    {
        db.AuditEvents.Add(new AuditEvent { ReportId = id, ActorId = actor, Action = action });
        await db.SaveChangesAsync(ct);
    }
}
