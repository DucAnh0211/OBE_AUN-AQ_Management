using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Presentation;

internal static class CourseOfferingEndpoints
{
    public static void MapCourseOfferingEndpoints(this RouteGroupBuilder root)
    {
        var group = root.MapGroup("/program-versions/{versionId:long:min(1)}").RequireAuthorization(AuthorizationPolicies.AdminOrLecturer);
        group.AddEndpointFilter(async (context, next) => { context.HttpContext.Response.Headers.CacheControl = "no-store"; return await next(context); });
        group.MapGet("/academic-periods", async (long versionId, CourseOfferingService s, CancellationToken ct) => Results.Ok(await s.PeriodsAsync(versionId, ct)));
        group.MapGet("/courses/{courseId:long:min(1)}/offerings", async (long versionId, long courseId, CourseOfferingService s, CancellationToken ct, int page = 1, string? search = null) => Results.Ok(await s.SubjectOfferingsAsync(versionId, courseId, page, search, ct)));
        group.MapGet("/classes", async (long versionId, string period, CourseOfferingService s, CancellationToken ct, int page = 1, string? search = null) => Results.Ok(await s.ClassesAsync(versionId, period, page, search, ct)));
        group.MapPost("/classes", async (long versionId, CreateAcademicClass r, CourseOfferingService s, CancellationToken ct) => Results.Ok(await s.CreateClassAsync(versionId, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/classes/{classId:long:min(1)}", async (long versionId, long classId, CourseOfferingService s, CancellationToken ct) => Results.Ok(await s.ClassAsync(versionId, classId, ct)));
        group.MapGet("/classes/{classId:long:min(1)}/offerings", async (long versionId, long classId, CourseOfferingService s, CancellationToken ct) => Results.Ok(await s.OfferingsAsync(versionId, classId, ct)));
        group.MapPost("/classes/{classId:long:min(1)}/offerings", async (long versionId, long classId, CreateCourseOffering r, CourseOfferingService s, CancellationToken ct) => Results.Ok(await s.CreateOfferingAsync(versionId, classId, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/classes/{classId:long:min(1)}/available-courses", async (long versionId, long classId, CourseOfferingService s, CancellationToken ct, int page = 1, string? search = null) => Results.Ok(await s.SubjectsAsync(versionId, classId, page, search, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/offering-lecturers", async (CourseOfferingService s, CancellationToken ct, string? search = null) => Results.Ok(await s.LecturersAsync(search, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/offerings/{offeringId:long:min(1)}", async (long versionId, long offeringId, CourseOfferingService s, CancellationToken ct) => Results.Ok(await s.AccessAsync(versionId, offeringId, false, ct)));
        group.MapPut("/offerings/{offeringId:long:min(1)}/lecturer", async (long versionId, long offeringId, AssignOfferingLecturer r, CourseOfferingService s, CancellationToken ct) => Results.Ok(await s.AssignAsync(versionId, offeringId, r.LecturerId, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
    }
}
