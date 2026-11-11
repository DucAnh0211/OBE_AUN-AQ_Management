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
        // Cơ cấu điểm thuộc đề cương từng học phần.
        group.MapGet("/courses/{courseId:long:min(1)}/assessment-scheme", async (long versionId, long courseId, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.SchemeAsync(versionId, courseId, ct)));
        group.MapPut("/courses/{courseId:long:min(1)}/assessment-scheme", async (long versionId, long courseId, SaveAssessmentScheme request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.SaveSchemeAsync(versionId, courseId, request, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPost("/assessment-scheme/apply-template", async (long versionId, ApplySchemeTemplate request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.ApplyTemplateAsync(versionId, request, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/assessment-coverage", async (long versionId, AcademicService service, CancellationToken ct) => Results.Ok(await service.CoverageAsync(versionId, ct)));
        group.MapGet("/grading-policy", async (long versionId, AcademicService service, CancellationToken ct) => Results.Ok(await service.PolicyAsync(versionId, ct)));

        var courses = group.MapGroup("/offerings/{offeringId:long:min(1)}");
        courses.MapGet("/academic-periods", async (long versionId, long offeringId, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.PeriodsAsync(versionId, offeringId, ct)));
        courses.MapGet("/assessment-scheme", async (long versionId, long offeringId, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.SchemeForOfferingAsync(versionId, offeringId, ct)));
        courses.MapGet("/students", async (long versionId, long offeringId, AcademicService service, CancellationToken ct, int page = 1, string? search = null) =>
            Results.Ok(await service.StudentsAsync(versionId, offeringId, page, search, ct)));
        courses.MapPost("/students", async (long versionId, long offeringId, EnrollStudent request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.EnrollAsync(versionId, offeringId, request, ct)));
        courses.MapGet("/students/{enrollmentId:long:min(1)}/grades", async (long versionId, long offeringId, long enrollmentId, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.GradesAsync(versionId, offeringId, enrollmentId, ct)));
        courses.MapPut("/students/{enrollmentId:long:min(1)}/grades", async (long versionId, long offeringId, long enrollmentId, SaveStudentGrades request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.SaveGradesAsync(versionId, offeringId, enrollmentId, request, ct)));
        courses.MapPost("/grades/{action:regex(^(submit|publish|reopen)$)}", async (long versionId, long offeringId, string action, GradeStatusChange? request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.ChangeGradeStatusAsync(versionId, offeringId, action, request ?? new(null), ct)));
        courses.MapGet("/import/template", async (long versionId, long offeringId, string mode, AcademicService service, IAcademicSheetReader sheets, CancellationToken ct) => {
            if (mode is not ("students" or "grades")) AcademicValidator.Invalid("mode", "Chọn loại mẫu sinh viên hoặc điểm.");
            var scheme = await service.SchemeForOfferingAsync(versionId, offeringId, ct);
            return Results.File(sheets.Template(mode, scheme), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{mode}-template.xlsx");
        });
        courses.MapPost("/import/read", async (long versionId, long offeringId, HttpRequest request, AcademicService service, IAcademicSheetReader sheets, CancellationToken ct) => {
            await service.CourseAccessAsync(versionId, offeringId, true, ct);
            var upload = await SheetUpload.ReadAsync(request, ct);
            if (upload.Error is not null) return Results.BadRequest(new { detail = upload.Error });
            var names = sheets.Sheets(upload.Bytes, upload.Name);
            if (upload.ListSheets) return Results.Ok(new { sheets = names });
            var scheme = await service.SchemeForOfferingAsync(versionId, offeringId, ct);
            var rows = sheets.Read(upload.Bytes, upload.Name, upload.Sheet, upload.Mode, scheme);
            return Results.Ok(new { rows, schemeRevision = scheme.Revision, sheets = names });
        }).DisableAntiforgery().WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(6 * 1024 * 1024));
        courses.MapPost("/import/preview", async (long versionId, long offeringId, AcademicImportRequest request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.ImportAsync(versionId, offeringId, request, false, ct)));
        courses.MapPost("/import/apply", async (long versionId, long offeringId, AcademicImportRequest request, AcademicService service, CancellationToken ct) =>
            Results.Ok(await service.ImportAsync(versionId, offeringId, request, true, ct)));
    }
}

/// <summary>Đọc tệp .xlsx/CSV hoặc bảng dán từ form import (tối đa 5 MB).</summary>
internal sealed record SheetUpload(byte[] Bytes, string Name, string? Sheet, string Mode, bool ListSheets, string? Error)
{
    public static async Task<SheetUpload> ReadAsync(HttpRequest request, CancellationToken ct)
    {
        if (!request.HasFormContentType || request.ContentLength > 6 * 1024 * 1024) return new([], "", null, "", false, "Dùng tệp hoặc bảng dán, tối đa 5 MB.");
        var form = await request.ReadFormAsync(ct); var file = form.Files.GetFile("file");
        byte[] bytes; string name;
        if (file is not null) {
            if (file.Length > 5 * 1024 * 1024) return new([], "", null, "", false, "Tệp vượt quá 5 MB.");
            using var buffer = new MemoryStream(); await file.CopyToAsync(buffer, ct); bytes = buffer.ToArray(); name = file.FileName;
        }
        else { bytes = System.Text.Encoding.UTF8.GetBytes(form["text"].ToString()); name = "pasted.tsv"; }
        return new(bytes, name, form["sheet"].FirstOrDefault(), form["mode"].ToString(), form["listSheets"] == "true", null);
    }
}
