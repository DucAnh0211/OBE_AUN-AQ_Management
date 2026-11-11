using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Presentation;

internal static class StudentDirectoryEndpoints
{
    public static void MapStudentDirectoryEndpoints(this RouteGroupBuilder root)
    {
        var version = root.MapGroup("/program-versions/{versionId:long:min(1)}/home-classes").RequireAuthorization(AuthorizationPolicies.AdminOrLecturer);
        version.AddEndpointFilter(async (context, next) => { context.HttpContext.Response.Headers.CacheControl = "no-store"; return await next(context); });
        version.MapGet("", async (long versionId, StudentDirectoryService s, CancellationToken ct, int page = 1, string? search = null) =>
            Results.Ok(await s.HomeClassesAsync(versionId, page, search, ct)));
        version.MapPost("", async (long versionId, SaveHomeClass r, StudentDirectoryService s, CancellationToken ct) =>
            Results.Ok(await s.SaveHomeClassAsync(versionId, null, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        version.MapGet("/{id:long:min(1)}", async (long versionId, long id, StudentDirectoryService s, CancellationToken ct) => Results.Ok(await s.HomeClassAsync(versionId, id, ct)));
        version.MapPut("/{id:long:min(1)}", async (long versionId, long id, SaveHomeClass r, StudentDirectoryService s, CancellationToken ct) =>
            Results.Ok(await s.SaveHomeClassAsync(versionId, id, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        version.MapPost("/{id:long:min(1)}/students", async (long versionId, long id, AddHomeClassStudent r, StudentDirectoryService s, CancellationToken ct) =>
            Results.Ok(await s.AddToHomeClassAsync(versionId, id, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        version.MapPost("/{id:long:min(1)}/students/import/read", async (long versionId, long id, HttpRequest request, StudentDirectoryService s, IAcademicSheetReader sheets, CancellationToken ct) => {
            await s.HomeClassAsync(versionId, id, ct);
            var upload = await SheetUpload.ReadAsync(request, ct);
            if (upload.Error is not null) return Results.BadRequest(new { detail = upload.Error });
            var names = sheets.Sheets(upload.Bytes, upload.Name);
            if (upload.ListSheets) return Results.Ok(new { sheets = names });
            var rows = sheets.Read(upload.Bytes, upload.Name, upload.Sheet, "students", new AssessmentScheme(0, 0, false, []));
            return Results.Ok(new { rows, schemeRevision = 0, sheets = names });
        }).DisableAntiforgery().WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(6 * 1024 * 1024)).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        version.MapPost("/{id:long:min(1)}/students/import/preview", async (long versionId, long id, HomeClassImportRequest r, StudentDirectoryService s, CancellationToken ct) =>
            Results.Ok(await s.ImportHomeClassAsync(versionId, id, r, false, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        version.MapPost("/{id:long:min(1)}/students/import/apply", async (long versionId, long id, HomeClassImportRequest r, StudentDirectoryService s, CancellationToken ct) =>
            Results.Ok(await s.ImportHomeClassAsync(versionId, id, r, true, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        var students = root.MapGroup("/universities/{universityId:long:min(1)}/students").RequireAuthorization(AuthorizationPolicies.Authenticated);
        students.AddEndpointFilter(async (context, next) => { context.HttpContext.Response.Headers.CacheControl = "no-store"; return await next(context); });
        students.MapGet("", async (long universityId, StudentDirectoryService s, CancellationToken ct, int page = 1, string? search = null, long? homeClassId = null, long? versionId = null) =>
            Results.Ok(await s.StudentsAsync(universityId, page, search, homeClassId, versionId, ct)));
        students.MapPut("/{id:long:min(1)}", async (long universityId, long id, UpdateStudent r, StudentDirectoryService s, CancellationToken ct) =>
            Results.Ok(await s.UpdateStudentAsync(universityId, id, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        students.MapGet("/{id:long:min(1)}/transcript", async (long universityId, long id, StudentDirectoryService s, CancellationToken ct) =>
            Results.Ok(await s.TranscriptAsync(universityId, id, ct)));

        root.MapGet("/student-accounts", async (StudentDirectoryService s, CancellationToken ct, string? search = null) =>
            Results.Ok(await s.StudentAccountsAsync(search, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        root.MapGet("/me/transcript", async (HttpContext http, StudentDirectoryService s, CancellationToken ct) => {
            http.Response.Headers.CacheControl = "no-store";
            return Results.Ok(await s.MyTranscriptAsync(ct));
        }).RequireAuthorization(AuthorizationPolicies.Authenticated);
    }
}
