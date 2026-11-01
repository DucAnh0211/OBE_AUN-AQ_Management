using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class EfKnowledgeRepository(KnowledgeDbContext db) : IKnowledgeRepository
{
    public Task<IngestionJob?> FindAsync(Guid id, CancellationToken ct) => db.Jobs.AsNoTracking().SingleOrDefaultAsync(x => x.DocumentId == id, ct);
    public async Task<IReadOnlyList<IngestionJob>> ListAsync(Guid[] ids, CancellationToken ct) => await db.Jobs.AsNoTracking().Where(x => ids.Contains(x.DocumentId)).ToArrayAsync(ct);
    public async Task QueueAsync(Guid id, string model, bool forceOcr, CancellationToken ct)
    {
        var existing = await FindAsync(id, ct);
        if (existing is null)
        {
            db.Jobs.Add(new IngestionJob { DocumentId = id, Model = model, ForceOcr = forceOcr, UpdatedAt = DateTime.UtcNow });
            try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { throw new KnowledgeException(409, "Tài liệu đã được xếp hàng. Hãy làm mới danh sách."); }
            return;
        }
        var changed = await db.Jobs.Where(x => x.DocumentId == id && x.Status == "failed")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "queued").SetProperty(x => x.Error, (string?)null)
                .SetProperty(x => x.ForceOcr, forceOcr).SetProperty(x => x.Model, model).SetProperty(x => x.Attempts, 0).SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        if (changed == 0) throw new KnowledgeException(409, "Chỉ có thể thử lại tài liệu xử lý lỗi.");
    }
    public async Task<IngestionJob?> ClaimAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        // A process crash leaves a lease; recovery is bounded, not an unbounded paid retry loop.
        await db.Jobs.Where(x => x.Status == "processing" && x.LeaseUntil < now && x.Attempts >= 3)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "failed").SetProperty(x => x.Error, "Xử lý bị gián đoạn nhiều lần. Hãy thử lại.").SetProperty(x => x.Lease, (Guid?)null), ct);
        var candidate = await db.Jobs.AsNoTracking().Where(x => x.Status == "queued" || (x.Status == "processing" && x.LeaseUntil < now && x.Attempts < 3))
            .OrderBy(x => x.UpdatedAt).FirstOrDefaultAsync(ct);
        if (candidate is null) return null;
        var lease = Guid.NewGuid();
        var count = await db.Jobs.Where(x => x.DocumentId == candidate.DocumentId && (x.Status == "queued" || (x.Status == "processing" && x.LeaseUntil < now && x.Attempts < 3)))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "processing").SetProperty(x => x.Lease, lease)
                .SetProperty(x => x.LeaseUntil, now.AddMinutes(15)).SetProperty(x => x.Attempts, x => x.Attempts + 1).SetProperty(x => x.UpdatedAt, now), ct);
        return count == 0 ? null : await FindAsync(candidate.DocumentId, ct);
    }
    public async Task CompleteAsync(IngestionJob job, IReadOnlyList<KnowledgeChunk> chunks, string? warning, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        // Fence expired/disabled workers and publish all chunks atomically.
        var changed = await db.Jobs.Where(x => x.DocumentId == job.DocumentId && x.Lease == job.Lease && x.Status == "processing")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "ready").SetProperty(x => x.ChunkCount, chunks.Count)
                .SetProperty(x => x.PipelineVersion, job.PipelineVersion).SetProperty(x => x.Dimensions, chunks[0].Embedding.Length).SetProperty(x => x.Warning, warning)
                .SetProperty(x => x.Error, (string?)null).SetProperty(x => x.Lease, (Guid?)null).SetProperty(x => x.LeaseUntil, (DateTime?)null)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        if (changed == 0) return;
        await db.Chunks.Where(x => x.DocumentId == job.DocumentId).ExecuteDeleteAsync(ct);
        db.Chunks.AddRange(chunks); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
    public async Task FailAsync(IngestionJob job, string message, CancellationToken ct) =>
        await db.Jobs.Where(x => x.DocumentId == job.DocumentId && x.Lease == job.Lease && x.Status == "processing")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "failed").SetProperty(x => x.Error, message)
                .SetProperty(x => x.Lease, (Guid?)null).SetProperty(x => x.LeaseUntil, (DateTime?)null).SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
    public async Task DisableAsync(Guid id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.Jobs.Where(x => x.DocumentId == id).ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "disabled")
            .SetProperty(x => x.Lease, (Guid?)null).SetProperty(x => x.ChunkCount, 0).SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        await db.Chunks.Where(x => x.DocumentId == id).ExecuteDeleteAsync(ct);
        await tx.CommitAsync(ct);
    }
    public async Task<IReadOnlyList<KnowledgeChunk>> ChunksAsync(Guid id, int page, CancellationToken ct) =>
        await db.Chunks.AsNoTracking().Where(x => x.DocumentId == id && db.Jobs.Any(j => j.DocumentId == id && j.Status == "ready"))
            .OrderBy(x => x.Ordinal).Skip((Math.Clamp(page, 1, 100) - 1) * 20).Take(20)
            .Select(x => new KnowledgeChunk { DocumentId = x.DocumentId, Ordinal = x.Ordinal, Text = x.Text, Locator = x.Locator, StartOffset = x.StartOffset }).ToArrayAsync(ct);
}
