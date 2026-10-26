using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Reporting.Application;

namespace ObeAunQa.Modules.Reporting;

internal static class ReportEndpoints
{
    public static void MapReports(this RouteGroupBuilder module)
    {
        var group = module.MapGroup("").RequireAuthorization("Reporting");
        group.MapGet("/session", (ClaimsPrincipal user) => Results.Ok(new {
            userId = user.FindFirstValue(ClaimTypes.NameIdentifier), programIds = ReportPermissions.Programs(user), canGenerate = ReportPermissions.CanGenerate(user)
        }));
        group.MapGet("/programs", async (ClaimsPrincipal user, CurriculumService curriculum, CancellationToken ct) => {
            var programs = new List<ProgramResponse>();
            foreach (var id in ReportPermissions.Programs(user))
            {
                try { var program = await curriculum.GetProgramAsync(id, ct); if (!program.IsArchived) programs.Add(program); }
                catch (CurriculumNotFoundException) { /* A removed program is no longer selectable. */ }
            }
            return Results.Ok(programs);
        });
        group.MapGet("/programs/{programId:long}/versions", async (long programId, ClaimsPrincipal user, CurriculumService curriculum, CancellationToken ct) => {
            ReportPermissions.Demand(user, programId);
            return Results.Ok(await curriculum.GetProgramVersionsAsync(programId, false, ct));
        });
        group.MapGet("/versions/{versionId:long}/courses", async (long versionId, ClaimsPrincipal user, CurriculumService curriculum, ICurriculumReportSource source, CancellationToken ct) => {
            var version = await curriculum.GetProgramVersionAsync(versionId, ct);
            ReportPermissions.Demand(user, version.ProgramId);
            var snapshot = await source.ReadAsync(versionId, ct) ?? throw new ReportException(404, "Phiên bản không tồn tại hoặc đã lưu trữ.");
            return Results.Ok(snapshot.Courses);
        });
        group.MapPost("/reports", async (CreateReportRequest request, ClaimsPrincipal user, ReportService service, CancellationToken ct) => {
            var report = await service.CreateAsync(request, user, ct);
            return Results.Created($"/api/reporting/reports/{report.Id}", report);
        }).RequireRateLimiting("report-work");
        group.MapGet("/reports", async (ClaimsPrincipal user, ReportService service, CancellationToken ct, int page = 1, int pageSize = 20) =>
            Results.Ok(await service.ListAsync(user, page, pageSize, ct)));
        group.MapGet("/reports/{id:guid}", async (Guid id, ClaimsPrincipal user, ReportService service, CancellationToken ct) =>
            Results.Ok(await service.GetAsync(id, user, ct)));
        group.MapGet("/reports/{id:guid}/download", async (Guid id, string format, ClaimsPrincipal user, ReportService service, CancellationToken ct) => {
            var file = await service.DownloadAsync(id, format, user, ct);
            return Results.File(file.Content, file.ContentType, file.FileName);
        }).RequireRateLimiting("report-work");
        group.MapPost("/reports/{id:guid}/revoke", async (Guid id, ClaimsPrincipal user, ReportService service, CancellationToken ct) => {
            await service.RevokeAsync(id, user, ct);
            return Results.NoContent();
        });
    }
}
