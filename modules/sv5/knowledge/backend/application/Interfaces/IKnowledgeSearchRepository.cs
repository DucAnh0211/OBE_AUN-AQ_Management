using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Application;

public interface IKnowledgeSearchRepository
{
    Task<IReadOnlyList<IngestionJob>> JobsAsync(Guid[] ids, CancellationToken ct);
    Task<IReadOnlyList<KnowledgeChunk>> ChunkBatchAsync(Guid[] ids, int offset, CancellationToken ct);
    Task<KnowledgeChunk?> FindChunkAsync(Guid id, int ordinal, CancellationToken ct);
    Task<IReadOnlyList<KnowledgeChunk>> SectionAsync(Guid id, string locator, CancellationToken ct);
}

