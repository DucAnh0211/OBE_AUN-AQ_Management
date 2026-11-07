using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Accreditation.Presentation;

internal static class AccreditationDirectoryEndpoints
{
    // Students browse this directory too, so it sits outside the admin/lecturer group.
    public static IEndpointRouteBuilder MapAccreditationDirectory(this IEndpointRouteBuilder endpoints, string prefix)
    {
        var directory = endpoints.MapGroup(prefix + "/directory").WithTags("Accreditation Directory")
            .RequireAuthorization(AuthorizationPolicies.PasswordChanged);
        directory.MapGet("/universities", GetUniversitiesAsync);
        directory.MapGet("/universities/{universityId:long:min(1)}/programs", GetProgramsAsync);
        directory.MapGet("/programs/{programId:long:min(1)}/accreditations", GetRecordsAsync);

        var records = endpoints.MapGroup(prefix).WithTags("Accreditation Directory")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        records.MapPost("/programs/{programId:long:min(1)}/accreditations", CreateAsync);
        records.MapPut("/program-accreditations/{id:long:min(1)}", UpdateAsync);
        records.MapDelete("/program-accreditations/{id:long:min(1)}", DeleteAsync);
        return endpoints;
    }

    private static async Task<long[]?> ScopeAsync(ICurrentUser user, IAccessControlService access, CancellationToken ct) =>
        user.Role == SystemRoles.Admin ? null : await access.GetReadableProgramIdsAsync(ct);

    private static async Task<IResult> GetUniversitiesAsync(
        ProgramAccreditationService service,
        ICurrentUser user,
        IAccessControlService access,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery(Name = "q")] string? search = null) =>
        Results.Ok(await service.GetUniversitiesAsync(page, pageSize, search, await ScopeAsync(user, access, ct), ct));

    private static async Task<IResult> GetProgramsAsync(
        long universityId,
        ProgramAccreditationService service,
        ICurrentUser user,
        IAccessControlService access,
        CancellationToken ct,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery(Name = "q")] string? search = null) =>
        Results.Ok(await service.GetProgramsAsync(universityId, status, page, pageSize, search, await ScopeAsync(user, access, ct), ct));

    private static async Task<IResult> GetRecordsAsync(
        long programId,
        ProgramAccreditationService service,
        IAccessControlService access,
        CancellationToken ct) =>
        await access.CanReadProgramAsync(programId, ct) ? Results.Ok(await service.GetRecordsAsync(programId, ct)) : Results.Forbid();

    private static async Task<IResult> CreateAsync(
        long programId,
        SaveProgramAccreditationRequest request,
        ProgramAccreditationService service,
        ICurrentUser user,
        CancellationToken ct)
    {
        var created = await service.CreateAsync(programId, request, user.UserId, ct);
        return Results.Created($"/api/accreditation/program-accreditations/{created.Id}", created);
    }

    private static async Task<IResult> UpdateAsync(
        long id,
        SaveProgramAccreditationRequest request,
        ProgramAccreditationService service,
        CancellationToken ct) =>
        Results.Ok(await service.UpdateAsync(id, request, ct));

    private static async Task<IResult> DeleteAsync(long id, ProgramAccreditationService service, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
