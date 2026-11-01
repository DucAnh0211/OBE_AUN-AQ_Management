using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObeAunQa.Modules.Knowledge.Application;
namespace ObeAunQa.Modules.Knowledge;

internal static class ChatAgentInternalEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/knowledge/internal/agent/{threadId:guid}/{runId:guid}").AllowAnonymous();
        group.AddEndpointFilter(async (context, next) => {
            var http = context.HttpContext;
            var configured = http.RequestServices.GetRequiredService<IConfiguration>()["AI_SERVICE_KEY"] ?? "";
            var provided = http.Request.Headers["X-Service-Key"].ToString();
            if (configured.Length < 32 || provided.Length > 512 || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(configured), Encoding.UTF8.GetBytes(provided)))
                return Results.Unauthorized();
            if (!Guid.TryParse(http.Request.Headers["X-Run-Lease"], out var lease)) return Results.Unauthorized();
            await http.RequestServices.GetRequiredService<IChatThreadRepository>().DemandLeaseAsync(
                Guid.Parse(http.Request.RouteValues["threadId"]!.ToString()!), Guid.Parse(http.Request.RouteValues["runId"]!.ToString()!), lease, http.RequestAborted);
            http.Items["ChatLease"] = lease; return await next(context);
        });
        group.MapGet("/checkpoint", async (Guid runId, IChatThreadRepository repository, CancellationToken ct, string? checkpointId = null) =>
            Results.Ok(await repository.CheckpointAsync(runId, checkpointId, ct)));
        group.MapPost("/checkpoint", async (Guid runId, CheckpointEnvelope value, IChatThreadRepository repository, HttpContext http, CancellationToken ct) => {
            await repository.PutCheckpointAsync(runId, (Guid)http.Items["ChatLease"]!, value, ct); return Results.NoContent();
        });
        group.MapPost("/writes", async (Guid runId, CheckpointWritesRequest value, IChatThreadRepository repository, HttpContext http, CancellationToken ct) => {
            await repository.PutWritesAsync(runId, (Guid)http.Items["ChatLease"]!, value, ct); return Results.NoContent();
        });
        group.MapPost("/phase/{phase}", async (Guid runId, string phase, IChatThreadRepository repository, HttpContext http, CancellationToken ct) => {
            if (phase is not ("route" or "conversation" or "retrieval" or "complete")) return Results.BadRequest();
            await repository.PhaseAsync(runId, (Guid)http.Items["ChatLease"]!, phase, ct); return Results.NoContent();
        });
        group.MapPost("/retrieve", async (DocumentSearchPlan plan, KnowledgeChatService service, CancellationToken ct) =>
            Results.Ok(await service.AskPlannedAsync(plan, ct)));
        group.MapPost("/curriculum", async (Guid threadId, CurriculumToolRequest query, IChatThreadRepository threads,
            ICurriculumChatSource source, CancellationToken ct) =>
            Results.Ok(await source.ReadAsync(await threads.OwnerAsync(threadId, ct), query, ct)));
    }
}
