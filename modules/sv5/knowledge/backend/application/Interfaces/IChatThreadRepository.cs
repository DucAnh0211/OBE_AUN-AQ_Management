using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Application;
public interface IChatThreadRepository
{
    Task<IReadOnlyList<ChatThreadInfo>> ListAsync(long owner, int page, CancellationToken ct);
    Task<ChatThreadInfo> CreateAsync(long owner, CancellationToken ct);
    Task<ChatThreadDetail> GetAsync(Guid id, long owner, CancellationToken ct);
    Task DeleteAsync(Guid id, long owner, CancellationToken ct);
    Task<ChatExchangeInfo> EnqueueAsync(Guid id, long owner, SendChatMessage message, CancellationToken ct);
    Task<ChatExchange?> ClaimAsync(CancellationToken ct);
    Task<IReadOnlyList<ChatTurn>> HistoryAsync(ChatExchange run, CancellationToken ct);
    Task CompleteAsync(ChatExchange run, ChatAnswer answer, CancellationToken ct);
    Task FailAsync(ChatExchange run, string error, CancellationToken ct);
    Task DemandLeaseAsync(Guid threadId, Guid runId, Guid lease, CancellationToken ct);
    Task<long> OwnerAsync(Guid threadId, CancellationToken ct);
    Task<CheckpointDetail?> CheckpointAsync(Guid runId, string? checkpointId, CancellationToken ct);
    Task PutCheckpointAsync(Guid runId, Guid lease, CheckpointEnvelope value, CancellationToken ct);
    Task PutWritesAsync(Guid runId, Guid lease, CheckpointWritesRequest value, CancellationToken ct);
    Task PhaseAsync(Guid runId, Guid lease, string phase, CancellationToken ct);
}

