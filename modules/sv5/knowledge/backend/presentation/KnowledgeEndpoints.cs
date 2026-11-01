using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Knowledge;

internal static class KnowledgeEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/knowledge").WithTags("Knowledge Base").RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/configuration", async (IKnowledgePipeline pipeline, CancellationToken ct) => Results.Ok(await pipeline.ConfigurationAsync(ct)));
        group.MapGet("/documents/{id:guid}", async (Guid id, IDocumentLibrary documents, IKnowledgeRepository repository, CancellationToken ct) => {
            var document = await documents.FindAsync(id, ct) ?? throw new KnowledgeException(404, "Không tìm thấy tài liệu.");
            var job = await repository.FindAsync(id, ct);
            return Results.Ok(new { document, status = document.DisabledAt is not null ? "disabled" : job?.Status ?? "uploaded",
                chunkCount = job?.ChunkCount ?? 0, dimensions = job?.Dimensions ?? 0, model = job?.Model,
                pipelineVersion = job?.PipelineVersion, error = job?.Error, warning = job?.Warning });
        });
        group.MapGet("/documents", async (IDocumentLibrary documents, IKnowledgeRepository repository, CancellationToken ct, int page = 1) => {
            var items = await documents.ListAsync(page, ct);
            var jobs = await repository.ListAsync(items.Select(x => x.Id).ToArray(), ct);
            return Results.Ok(items.Select(d => { var j = jobs.SingleOrDefault(x => x.DocumentId == d.Id); return new {
                document = d, status = d.DisabledAt is not null ? "disabled" : j?.Status ?? "uploaded", chunkCount = j?.ChunkCount ?? 0,
                dimensions = j?.Dimensions ?? 0, model = j?.Model, pipelineVersion = j?.PipelineVersion, error = j?.Error, warning = j?.Warning }; }));
        });
        // Read multipart explicitly after bearer authorization; auth cookies do not authenticate this endpoint.
        group.MapPost("/documents", async (HttpContext http, IDocumentLibrary documents, ICurrentUser user, CancellationToken ct) => {
            if (!http.Request.HasFormContentType) throw new KnowledgeException(400, "Yêu cầu phải chứa file multipart.");
            if (http.Request.ContentLength > 21 * 1024 * 1024) throw new KnowledgeException(413, "File vượt giới hạn 20 MB.");
            var feature = http.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (feature is { IsReadOnly: false }) feature.MaxRequestBodySize = 21 * 1024 * 1024;
            var form = await http.Request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");
            if (file is null || form.Files.Count != 1 || file.Length is <= 0 or > 20 * 1024 * 1024)
                throw new KnowledgeException(400, "Chọn một file PDF, DOCX, TXT hoặc Markdown, tối đa 20 MB.");
            using var buffer = new MemoryStream(); await file.CopyToAsync(buffer, ct);
            var document = await documents.UploadAsync(form["title"].ToString(), file.FileName, buffer.ToArray(), user.UserId, ct);
            return Results.Created($"/api/knowledge/documents/{document.Id}", document);
        }).RequireRateLimiting("report-work");
        group.MapPost("/documents/{id:guid}/process", async (Guid id, IDocumentLibrary documents, IKnowledgeRepository repository, IKnowledgePipeline pipeline, CancellationToken ct, bool forceOcr = false) => {
            await Active(documents, id, ct);
            var configuration = await pipeline.ConfigurationAsync(ct);
            if (!configuration.EmbeddingConfigured) throw new KnowledgeException(503, configuration.Error ?? "Chưa cấu hình LLM_API_KEY trên Python service. File gốc đã lưu; có thể xử lý sau.");
            await repository.QueueAsync(id, configuration.EmbeddingModel, forceOcr, ct);
            // Close the race where source was disabled while the job was being queued.
            if ((await documents.FindAsync(id, ct))?.DisabledAt is not null)
            {
                await repository.DisableAsync(id, ct);
                throw new KnowledgeException(410, "Tài liệu đã bị vô hiệu hóa.");
            }
            return Results.Accepted($"/api/knowledge/documents/{id}/chunks", new { documentId = id, status = "queued" });
        }).RequireRateLimiting("report-work");
        group.MapGet("/documents/{id:guid}/original", async (Guid id, HttpContext http, IDocumentLibrary documents, CancellationToken ct) => {
            var d = await Active(documents, id, ct);
            var bytes = await documents.ReadAsync(id, ct);
            await Active(documents, id, ct);
            http.Response.Headers.CacheControl = "no-store";
            http.Response.Headers.XContentTypeOptions = "nosniff";
            return Results.File(bytes, "application/octet-stream", d.FileName);
        });
        group.MapGet("/documents/{id:guid}/chunks", async (Guid id, IDocumentLibrary documents, IKnowledgeRepository repository, CancellationToken ct, int page = 1) => {
            await Active(documents, id, ct);
            var chunks = await repository.ChunksAsync(id, page, ct);
            await Active(documents, id, ct);
            return Results.Ok(chunks.Select(c => new { c.Ordinal, c.Text, c.Locator, c.StartOffset }));
        });
        group.MapPost("/documents/{id:guid}/disable", async (Guid id, IDocumentLibrary documents, IKnowledgeRepository repository, ICurrentUser user, CancellationToken ct) => {
            // Source invalidation wins even if processing cleanup subsequently fails.
            await documents.DisableAsync(id, user.UserId, ct);
            await repository.DisableAsync(id, ct);
            return Results.NoContent();
        });
    }
    private static async Task<SourceDocumentInfo> Active(IDocumentLibrary documents, Guid id, CancellationToken ct)
    {
        var d = await documents.FindAsync(id, ct) ?? throw new KnowledgeException(404, "Không tìm thấy tài liệu.");
        if (d.DisabledAt is not null) throw new KnowledgeException(410, "Tài liệu đã bị vô hiệu hóa.");
        return d;
    }
}
