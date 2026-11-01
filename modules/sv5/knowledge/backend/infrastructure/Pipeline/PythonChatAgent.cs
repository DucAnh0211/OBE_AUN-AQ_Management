using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ObeAunQa.Modules.Knowledge.Application;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class PythonChatAgent(HttpClient http, IOptions<KnowledgeAiOptions> options) : IChatAgent
{
    public async Task<ChatAnswer> RunAsync(AgentRunRequest value, CancellationToken ct)
    {
        if (options.Value.SharedKey.Length < 32) throw new KnowledgeException(503, "Chưa cấu hình kết nối AI service.");
        using var request = new HttpRequestMessage(HttpMethod.Post, options.Value.Url.TrimEnd('/') + "/internal/knowledge/agent");
        request.Headers.Add("X-Service-Key", options.Value.SharedKey); request.Content = JsonContent.Create(value);
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            var detail = body.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : null;
            throw new KnowledgeException(502, detail is { Length: > 0 and <= 500 } ? detail : "Trợ lý chưa xử lý được tin nhắn. Bạn thử lại nhé.");
        }
        return await response.Content.ReadFromJsonAsync<ChatAnswer>(ct) ?? throw new KnowledgeException(502, "Chatbot không trả nội dung.");
    }
}
