using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Curriculum.Presentation;

internal static class UniversityEndpoints
{
    public static RouteGroupBuilder MapUniversityEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/universities", GetUniversitiesAsync);
        group.MapPost("/universities", CreateUniversityAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/universities/{universityId:long:min(1)}", GetUniversityAsync)
            .WithName("GetCurriculumUniversity");
        group.MapPut("/universities/{universityId:long:min(1)}", UpdateUniversityAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapDelete("/universities/{universityId:long:min(1)}", ArchiveUniversityAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        return group;
    }

    private static async Task<IResult> GetUniversitiesAsync(
        UniversityService service,
        ICurrentUser currentUser,
        IAccessControlService access,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery(Name = "q")] string? search = null,
        [FromQuery] bool includeArchived = false)
    {
        // Non-admins only see universities that own a program they can already read.
        var scope = currentUser.Role == SystemRoles.Admin ? null : await access.GetReadableProgramIdsAsync(cancellationToken);
        return Results.Ok(await service.GetUniversitiesAsync(page, pageSize, search, scope is null && includeArchived, scope, cancellationToken));
    }

    private static async Task<IResult> CreateUniversityAsync(
        CreateUniversityRequest request,
        UniversityService service,
        CancellationToken cancellationToken)
    {
        var university = await service.CreateUniversityAsync(request, cancellationToken);
        return Results.CreatedAtRoute("GetCurriculumUniversity", new { universityId = university.Id }, university);
    }

    private static async Task<IResult> GetUniversityAsync(
        long universityId,
        UniversityService service,
        ICurrentUser currentUser,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role != SystemRoles.Admin)
        {
            var readable = await access.GetReadableProgramIdsAsync(cancellationToken);
            var owned = await service.GetProgramIdsAsync([universityId], cancellationToken);
            if (!owned[universityId].Intersect(readable).Any()) return Results.Forbid();
        }

        return Results.Ok(await service.GetUniversityAsync(universityId, cancellationToken));
    }

    private static async Task<IResult> UpdateUniversityAsync(
        long universityId,
        UpdateUniversityRequest request,
        UniversityService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.UpdateUniversityAsync(universityId, request, cancellationToken));

    private static async Task<IResult> ArchiveUniversityAsync(
        long universityId,
        UniversityService service,
        CancellationToken cancellationToken)
    {
        await service.ArchiveUniversityAsync(universityId, cancellationToken);
        return Results.NoContent();
    }
}
