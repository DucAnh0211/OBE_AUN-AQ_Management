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
        if (options.Value.SharedKey.Length < 32) throw new KnowledgeException(503, "Chưa cấu hình AI_SERVICE_KEY.");
        using var request = new HttpRequestMessage(HttpMethod.Post, options.Value.Url.TrimEnd('/') + "/internal/knowledge/" + path);
        request.Headers.Add("X-Service-Key", options.Value.SharedKey);
        request.Content = JsonContent.Create(body);
        try
        {
            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
                var message = error.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : null;
                throw new KnowledgeException(502, message is { Length: > 0 and <= 500 } ? message : "Python chưa xử lý được yêu cầu chat.");
            }
            return await response.Content.ReadFromJsonAsync<T>(ct) ?? throw new KnowledgeException(502, "Python không trả kết quả.");
        }
        catch (HttpRequestException) { throw new KnowledgeException(503, "Chưa kết nối được dịch vụ Python."); }
        catch (JsonException) { throw new KnowledgeException(502, "Phản hồi Python không hợp lệ."); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new KnowledgeException(504, "Chatbot xử lý quá thời gian. Hãy thử lại."); }
    }
}
