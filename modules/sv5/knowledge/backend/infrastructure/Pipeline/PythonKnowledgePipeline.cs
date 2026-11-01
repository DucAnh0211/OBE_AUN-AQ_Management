using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ObeAunQa.Modules.Knowledge.Application;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class PythonKnowledgePipeline(HttpClient http, IOptions<KnowledgeAiOptions> options) : IKnowledgePipeline
{
    private HttpRequestMessage Request(HttpMethod method, string path)
    {
        if (options.Value.SharedKey.Length < 32) throw new KnowledgeException(503, "Chưa cấu hình AI_SERVICE_KEY (ít nhất 32 ký tự) cho .NET và Python.");
        var request = new HttpRequestMessage(method, options.Value.Url.TrimEnd('/') + path);
        request.Headers.Add("X-Service-Key", options.Value.SharedKey);
        return request;
    }
    public async Task<PipelineConfiguration> ConfigurationAsync(CancellationToken ct)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(4));
            using var request = Request(HttpMethod.Get, "/internal/knowledge/configuration");
            using var response = await http.SendAsync(request, timeout.Token);
            if (!response.IsSuccessStatusCode) return new(false, "", false, "Không truy cập được AI service. Kiểm tra AI_SERVICE_URL và AI_SERVICE_KEY.");
            return await response.Content.ReadFromJsonAsync<PipelineConfiguration>(timeout.Token)
                ?? new(false, "", false, "AI service trả cấu hình không hợp lệ.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return new(false, "", false, "AI service chưa phản hồi."); }
        catch (HttpRequestException) { return new(false, "", false, "AI service Python chưa chạy hoặc chưa kết nối được."); }
        catch (KnowledgeException e) { return new(false, "", false, e.Message); }
    }
    public async Task<PipelineResult> ProcessAsync(string fileName, byte[] bytes, string model, bool forceOcr, CancellationToken ct)
    {
        using var request = Request(HttpMethod.Post, "/internal/knowledge/process");
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes), "file", fileName);
        form.Add(new StringContent(model), "model");
        form.Add(new StringContent(forceOcr ? "true" : "false"), "force_ocr");
        request.Content = form;
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            // Python exposes only controlled messages, never upstream provider bodies.
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>(ct);
            var detail = body?.GetValueOrDefault("detail");
            var error = detail?.ValueKind == System.Text.Json.JsonValueKind.String ? detail.Value.GetString() : null;
            throw new KnowledgeException(502, error is { Length: > 0 and <= 500 } ? error : "AI service không xử lý được tài liệu.");
        }
        return await response.Content.ReadFromJsonAsync<PipelineResult>(ct) ?? throw new KnowledgeException(502, "AI service không trả kết quả.");
    }
}
