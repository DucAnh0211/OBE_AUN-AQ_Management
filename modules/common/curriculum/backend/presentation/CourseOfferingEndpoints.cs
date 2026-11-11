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
        group.MapGet("/courses/{courseId:long:min(1)}/offerings", async (long versionId, long courseId, CourseOfferingService s, CancellationToken ct, int page = 1, string? search = null) =>
            Results.Ok(await s.SubjectOfferingsAsync(versionId, courseId, page, search, ct)));
        group.MapGet("/offerings", async (long versionId, string period, CourseOfferingService s, CancellationToken ct, int page = 1, string? search = null) =>
            Results.Ok(await s.OfferingsAsync(versionId, period, page, search, ct)));
        group.MapPost("/offerings", async (long versionId, CreateCourseOffering r, CourseOfferingService s, CancellationToken ct) =>
            Results.Ok(await s.CreateOfferingAsync(versionId, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/available-courses", async (long versionId, CourseOfferingService s, CancellationToken ct, int page = 1, string? search = null) =>
            Results.Ok(await s.SubjectsAsync(versionId, page, search, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/offering-lecturers", async (CourseOfferingService s, CancellationToken ct, string? search = null) =>
            Results.Ok(await s.LecturersAsync(search, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/offerings/{offeringId:long:min(1)}", async (long versionId, long offeringId, CourseOfferingService s, CancellationToken ct) =>
            Results.Ok(await s.AccessAsync(versionId, offeringId, false, ct)));
        group.MapPut("/offerings/{offeringId:long:min(1)}", async (long versionId, long offeringId, UpdateCourseOffering r, CourseOfferingService s, CancellationToken ct) =>
            Results.Ok(await s.UpdateOfferingAsync(versionId, offeringId, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPut("/offerings/{offeringId:long:min(1)}/lecturers", async (long versionId, long offeringId, SaveOfferingLecturers r, CourseOfferingService s, CancellationToken ct) =>
            Results.Ok(await s.SaveLecturersAsync(versionId, offeringId, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPost("/offerings/{offeringId:long:min(1)}/enroll-home-class", async (long versionId, long offeringId, EnrollHomeClass r, CourseOfferingService s, CancellationToken ct) =>
            Results.Ok(await s.EnrollHomeClassAsync(versionId, offeringId, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
    }
}
