using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Presentation;

internal static class OrganizationEndpoints
{
    public static void MapOrganizationEndpoints(this RouteGroupBuilder root)
    {
        var university = root.MapGroup("/universities/{universityId:long:min(1)}").RequireAuthorization(AuthorizationPolicies.AdminOrLecturer);
        university.MapGet("/org-units", async (long universityId, OrganizationService s, CancellationToken ct, bool includeArchived = false) =>
            Results.Ok(await s.OrgUnitsAsync(universityId, includeArchived, ct)));
        university.MapPost("/org-units", async (long universityId, SaveOrgUnit r, OrganizationService s, CancellationToken ct) =>
            Results.Ok(await s.SaveOrgUnitAsync(universityId, null, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        university.MapPut("/org-units/{id:long:min(1)}", async (long universityId, long id, SaveOrgUnit r, OrganizationService s, CancellationToken ct) =>
            Results.Ok(await s.SaveOrgUnitAsync(universityId, id, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        university.MapDelete("/org-units/{id:long:min(1)}", async (long universityId, long id, OrganizationService s, CancellationToken ct) => {
            await s.ArchiveOrgUnitAsync(universityId, id, ct); return Results.NoContent();
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        university.MapGet("/grading-policy", async (long universityId, OrganizationService s, CancellationToken ct) => Results.Ok(await s.PolicyAsync(universityId, ct)));
        university.MapPut("/grading-policy", async (long universityId, SaveGradingPolicy r, OrganizationService s, CancellationToken ct) =>
            Results.Ok(await s.SavePolicyAsync(universityId, r, ct))).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        var admin = root.MapGroup("").RequireAuthorization(AuthorizationPolicies.AdminOnly);
        admin.MapGet("/lecturers", async (OrganizationService s, CancellationToken ct, string? search = null) => Results.Ok(await s.LecturersAsync(search, ct)));
        admin.MapPut("/lecturers/{userId:long:min(1)}", async (long userId, SaveLecturerProfile r, OrganizationService s, CancellationToken ct) =>
            Results.Ok(await s.SaveLecturerAsync(userId, r, ct)));
        admin.MapPut("/programs/{programId:long:min(1)}/faculty", async (long programId, AssignOrgUnit r, OrganizationService s, CancellationToken ct) => {
            await s.AssignFacultyAsync(programId, r, ct); return Results.NoContent();
        });
        admin.MapPut("/program-versions/{versionId:long:min(1)}/courses/{courseId:long:min(1)}/catalog", async (long versionId, long courseId, UpdateCourseCatalog r,
            OrganizationService s, CancellationToken ct) => { await s.AssignCourseCatalogAsync(versionId, courseId, r, ct); return Results.NoContent(); });
    }
}
