using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Application;
public interface IKnowledgeRepository
{
    Task<IngestionJob?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<IngestionJob>> ListAsync(Guid[] ids, CancellationToken ct);
    Task QueueAsync(Guid id, string model, bool forceOcr, CancellationToken ct);
    Task<IngestionJob?> ClaimAsync(CancellationToken ct);
    Task CompleteAsync(IngestionJob job, IReadOnlyList<KnowledgeChunk> chunks, string? warning, CancellationToken ct);
    Task FailAsync(IngestionJob job, string message, CancellationToken ct);
    Task DisableAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<KnowledgeChunk>> ChunksAsync(Guid id, int page, CancellationToken ct);
}
