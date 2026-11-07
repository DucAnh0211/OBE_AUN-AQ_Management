using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Presentation;

internal static class AcademicEndpoints
{
    public static void MapAcademicEndpoints(this RouteGroupBuilder curriculum)
    {
        var group = curriculum.MapGroup("/program-versions/{versionId:long:min(1)}").RequireAuthorization(AuthorizationPolicies.AdminOrLecturer);
        group.AddEndpointFilter(async (context, next) => { context.HttpContext.Response.Headers.CacheControl = "no-store"; return await next(context); });
        group.MapGet("/assessment-scheme", async (long versionId, AcademicService service, CancellationToken ct) => Results.Ok(await service.SchemeAsync(versionId, ct)));
        group.MapPut("/assessment-scheme", async (long versionId, SaveAssessmentScheme request, AcademicService service, CancellationToken ct) => Results.Ok(await service.SaveSchemeAsync(versionId, request, ct)))
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        var courses = group.MapGroup("/offerings/{offeringId:long:min(1)}");
        courses.MapGet("/academic-periods", async (long versionId, long offeringId, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.PeriodsAsync(versionId, offeringId, ct)));
        courses.MapGet("/students", async (long versionId, long offeringId, string period, AcademicService service, CancellationToken ct, int page = 1, string? search = null) =>
            Results.Ok(await service.StudentsAsync(versionId, offeringId, period, page, search, ct)));
        courses.MapPost("/students", async (long versionId, long offeringId, EnrollStudent request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.EnrollAsync(versionId, offeringId, request, ct)));
        courses.MapGet("/students/{enrollmentId:long:min(1)}/grades", async (long versionId, long offeringId, long enrollmentId, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.GradesAsync(versionId, offeringId, enrollmentId, ct)));
        courses.MapPut("/students/{enrollmentId:long:min(1)}/grades", async (long versionId, long offeringId, long enrollmentId, SaveStudentGrades request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.SaveGradesAsync(versionId, offeringId, enrollmentId, request, ct)));
        courses.MapGet("/import/template", async (long versionId, long offeringId, string mode, AcademicService service, IAcademicSheetReader sheets, CancellationToken ct) => {
            await service.CourseAccessAsync(versionId, offeringId, false, ct);
            if (mode is not ("students" or "grades")) AcademicValidator.Invalid("mode", "Chọn loại mẫu sinh viên hoặc điểm.");
            var scheme = await service.SchemeAsync(versionId, ct);
            return Results.File(sheets.Template(mode, scheme), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{mode}-template.xlsx");
        });
        courses.MapPost("/import/read", async (long versionId, long offeringId, HttpRequest request, AcademicService service, IAcademicSheetReader sheets, CancellationToken ct) => {
            await service.CourseAccessAsync(versionId, offeringId, true, ct);
            if (!request.HasFormContentType || request.ContentLength > 6 * 1024 * 1024) return Results.BadRequest(new { detail = "Dùng tệp hoặc bảng dán, tối đa 5 MB." });
            var form = await request.ReadFormAsync(ct); var file = form.Files.GetFile("file");
            byte[] bytes; string name;
            if (file is not null) {
                if (file.Length > 5 * 1024 * 1024) return Results.BadRequest(new { detail = "Tệp vượt quá 5 MB." });
                using var buffer = new MemoryStream(); await file.CopyToAsync(buffer, ct); bytes = buffer.ToArray(); name = file.FileName;
            }
            else { bytes = System.Text.Encoding.UTF8.GetBytes(form["text"].ToString()); name = "pasted.tsv"; }
            var names = sheets.Sheets(bytes, name);
            if (form["listSheets"] == "true") return Results.Ok(new { sheets = names });
            var mode = form["mode"].ToString(); var scheme = await service.SchemeAsync(versionId, ct);
            var rows = sheets.Read(bytes, name, form["sheet"].FirstOrDefault(), mode, scheme);
            return Results.Ok(new { rows, schemeRevision = scheme.Revision, sheets = names });
        }).DisableAntiforgery().WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(6 * 1024 * 1024));
        courses.MapPost("/import/preview", async (long versionId, long offeringId, AcademicImportRequest request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.ImportAsync(versionId, offeringId, request, false, ct)));
        courses.MapPost("/import/apply", async (long versionId, long offeringId, AcademicImportRequest request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.ImportAsync(versionId, offeringId, request, true, ct)));
    }
}
