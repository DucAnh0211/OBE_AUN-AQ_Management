using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Knowledge;

internal static class KnowledgeChatEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/knowledge/chat").WithTags("Knowledge Chat")
            .RequireAuthorization(AuthorizationPolicies.AdminOrLecturer);
        group.MapGet("/threads", async (IChatThreadRepository repository, ICurrentUser user, CancellationToken ct, int page = 1) =>
            Results.Ok(await repository.ListAsync(user.UserId, page, ct)));
        group.MapPost("/threads", async (IChatThreadRepository repository, ICurrentUser user, CancellationToken ct) => {
            var thread = await repository.CreateAsync(user.UserId, ct); return Results.Created($"/api/knowledge/chat/threads/{thread.Id}", thread);
        }).RequireRateLimiting("report-work");
        group.MapGet("/threads/{id:guid}", async (Guid id, IChatThreadRepository repository, ICurrentUser user, CancellationToken ct) =>
            Results.Ok(await repository.GetAsync(id, user.UserId, ct)));
        group.MapDelete("/threads/{id:guid}", async (Guid id, IChatThreadRepository repository, ICurrentUser user, CancellationToken ct) => {
            await repository.DeleteAsync(id, user.UserId, ct); return Results.NoContent();
        });
        group.MapPost("/threads/{id:guid}/messages", async (Guid id, SendChatMessage message, IChatThreadRepository repository, ICurrentUser user, CancellationToken ct) =>
            Results.Accepted($"/api/knowledge/chat/threads/{id}", await repository.EnqueueAsync(id, user.UserId, message, ct))).RequireRateLimiting("report-work");
        group.MapPost("/ask", async (ChatRequest request, KnowledgeChatService service, CancellationToken ct) =>
            Results.Ok(await service.AskAsync(request, ct))).RequireRateLimiting("report-work");
        group.MapGet("/sources/{id:guid}/{ordinal:int:min(1)}", async (Guid id, int ordinal, string sourceHash, string chunkHash,
            KnowledgeChatService service, CancellationToken ct) => Results.Ok(await service.SourceAsync(id, ordinal, sourceHash, chunkHash, ct)));
        group.MapGet("/sources/{id:guid}/original", async (Guid id, string sourceHash, KnowledgeChatService service, HttpContext http, CancellationToken ct) => {
            http.Response.Headers.CacheControl = "no-store";
            http.Response.Headers.XContentTypeOptions = "nosniff";
            return Results.File(await service.OriginalAsync(id, sourceHash, ct), "application/octet-stream");
        });
    }
}
