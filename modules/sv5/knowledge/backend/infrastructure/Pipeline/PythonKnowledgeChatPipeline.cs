using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class PythonKnowledgeChatPipeline(HttpClient http, IOptions<KnowledgeAiOptions> options) : IKnowledgeChatPipeline
{
    public Task<DocumentSearchPlan> PlanAsync(string question, IReadOnlyList<ChatTurn> history, CancellationToken ct) =>
        Post<DocumentSearchPlan>("document-keywords", new { question, history }, ct);
    public Task<QueryEmbedding> EmbedAsync(string question, CancellationToken ct) => Post<QueryEmbedding>("embed-query", new { question }, ct);
    public async Task<IReadOnlyList<RankedChunk>> RankAsync(float[] query, IReadOnlyList<KnowledgeChunk> chunks, CancellationToken ct) =>
        await Post<RankedChunk[]>("semantic-rank", new { query, chunks = chunks.Select(c => new { key = KnowledgeChatService.Key(c), embedding = c.Embedding }) }, ct);
    public Task<AiAnswer> AnswerAsync(string question, IReadOnlyList<EvidenceCandidate> candidates, CancellationToken ct) =>
        Post<AiAnswer>("answer", new { question, candidates }, ct);
    private async Task<T> Post<T>(string path, object body, CancellationToken ct)
    {
        try {
            return await ObeAunQa.SharedKernel.AiHttp.PostAsync<T>(http,options.Value.Url,options.Value.SharedKey,
                "internal/knowledge/"+path,body,ct);
        } catch(ObeAunQa.SharedKernel.AiServiceException ex) {
            throw new KnowledgeException(ex.StatusCode,ex.Message);
        }
    }
}
