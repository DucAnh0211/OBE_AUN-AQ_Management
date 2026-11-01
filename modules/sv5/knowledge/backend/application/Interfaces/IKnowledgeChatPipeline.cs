using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Application;

public interface IKnowledgeChatPipeline
{
    Task<DocumentSearchPlan> PlanAsync(string question, IReadOnlyList<ChatTurn> history, CancellationToken ct);
    Task<QueryEmbedding> EmbedAsync(string question, CancellationToken ct);
    Task<IReadOnlyList<RankedChunk>> RankAsync(float[] query, IReadOnlyList<KnowledgeChunk> chunks, CancellationToken ct);
    Task<AiAnswer> AnswerAsync(string question, IReadOnlyList<EvidenceCandidate> candidates, CancellationToken ct);
}

