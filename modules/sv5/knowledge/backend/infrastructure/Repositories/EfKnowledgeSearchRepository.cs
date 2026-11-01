using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class EfKnowledgeSearchRepository(KnowledgeDbContext db) : IKnowledgeSearchRepository
{
    public async Task<IReadOnlyList<IngestionJob>> JobsAsync(Guid[] ids, CancellationToken ct) =>
        await db.Jobs.AsNoTracking().Where(j => ids.Contains(j.DocumentId) && j.Status == "ready").ToArrayAsync(ct);
    public async Task<IReadOnlyList<KnowledgeChunk>> ChunkBatchAsync(Guid[] ids, int offset, CancellationToken ct) =>
        await db.Chunks.AsNoTracking().Where(c => ids.Contains(c.DocumentId) && db.Jobs.Any(j => j.DocumentId == c.DocumentId && j.Status == "ready"))
            .OrderBy(c => c.DocumentId).ThenBy(c => c.Ordinal).Skip(offset).Take(128).ToArrayAsync(ct);
    public Task<KnowledgeChunk?> FindChunkAsync(Guid id, int ordinal, CancellationToken ct) =>
        db.Chunks.AsNoTracking().SingleOrDefaultAsync(c => c.DocumentId == id && c.Ordinal == ordinal
            && db.Jobs.Any(j => j.DocumentId == id && j.Status == "ready"), ct);
    public async Task<IReadOnlyList<KnowledgeChunk>> SectionAsync(Guid id, string locator, CancellationToken ct) =>
        await db.Chunks.AsNoTracking().Where(c => c.DocumentId == id && c.Locator == locator)
            .OrderBy(c => c.StartOffset).Select(c => new KnowledgeChunk { DocumentId = c.DocumentId, Ordinal = c.Ordinal,
                Text = c.Text, Locator = c.Locator, StartOffset = c.StartOffset }).ToArrayAsync(ct);
}
