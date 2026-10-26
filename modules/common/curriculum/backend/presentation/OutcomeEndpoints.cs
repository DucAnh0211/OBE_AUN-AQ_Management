using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Curriculum.Presentation;

internal static class OutcomeEndpoints
{
    public static RouteGroupBuilder MapOutcomeEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/program-versions/{versionId:long:min(1)}/plos", GetPlosAsync);
        group.MapPost("/program-versions/{versionId:long:min(1)}/plos", CreatePloAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/program-versions/{versionId:long:min(1)}/plos/{ploId:long:min(1)}", GetPloAsync)
            .WithName("GetCurriculumPlo");
        group.MapPut("/program-versions/{versionId:long:min(1)}/plos/{ploId:long:min(1)}", UpdatePloAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapDelete("/program-versions/{versionId:long:min(1)}/plos/{ploId:long:min(1)}", DeletePloAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        var coursePath = "/program-versions/{versionId:long:min(1)}/courses/{programCourseId:long:min(1)}";
        group.MapGet(coursePath + "/clos", GetClosAsync);
        group.MapPost(coursePath + "/clos", CreateCloAsync);
        group.MapGet(coursePath + "/clos/{cloId:long:min(1)}", GetCloAsync)
            .WithName("GetCurriculumClo");
        group.MapPut(coursePath + "/clos/{cloId:long:min(1)}", UpdateCloAsync);
        group.MapDelete(coursePath + "/clos/{cloId:long:min(1)}", DeleteCloAsync);

        group.MapGet(coursePath + "/plo-mappings", GetCoursePloMappingsAsync);
        group.MapPost(coursePath + "/plo-mappings", CreateCoursePloMappingAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPut(
            coursePath + "/plo-mappings/{mappingId:long:min(1)}",
            UpdateCoursePloMappingAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapDelete(
            coursePath + "/plo-mappings/{mappingId:long:min(1)}",
            DeleteCoursePloMappingAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        var cloMappingPath = coursePath + "/clos/{cloId:long:min(1)}/plo-mappings";
        group.MapGet(cloMappingPath, GetCloPloMappingsAsync);
        group.MapPost(cloMappingPath, CreateCloPloMappingAsync);
        group.MapDelete(
            cloMappingPath + "/{coursePloId:long:min(1)}",
            DeleteCloPloMappingAsync);

        group.MapGet(
            "/program-versions/{versionId:long:min(1)}/plo-credit-check",
            GetPloCreditCheckAsync);
        group.MapGet(
            "/program-versions/{versionId:long:min(1)}/plo-balance",
            GetPloBalanceAsync);

        return group;
    }

    private static async Task<IResult> GetPlosAsync(
        long versionId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadVersionAsync(versionId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetPlosAsync(versionId, cancellationToken));
    }

    private static async Task<IResult> GetPloAsync(
        long versionId,
        long ploId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadVersionAsync(versionId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetPloAsync(versionId, ploId, cancellationToken));
    }

    private static async Task<IResult> CreatePloAsync(
        long versionId,
        CreatePloRequest request,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        var plo = await service.CreatePloAsync(versionId, request, cancellationToken);
        return Results.CreatedAtRoute(
            "GetCurriculumPlo",
            new { versionId, ploId = plo.Id },
            plo);
    }

    private static async Task<IResult> UpdatePloAsync(
        long versionId,
        long ploId,
        UpdatePloRequest request,
        CurriculumService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.UpdatePloAsync(versionId, ploId, request, cancellationToken));

    private static async Task<IResult> DeletePloAsync(
        long versionId,
        long ploId,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        await service.DeletePloAsync(versionId, ploId, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetClosAsync(
        long versionId,
        long programCourseId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadCourseAsync(programCourseId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetClosAsync(versionId, programCourseId, cancellationToken));
    }

    private static async Task<IResult> GetCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadCourseAsync(programCourseId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetCloAsync(
            versionId,
            programCourseId,
            cloId,
            cancellationToken));
    }

    private static async Task<IResult> CreateCloAsync(
        long versionId,
        long programCourseId,
        CreateCloRequest request,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanEditCourseOutcomesAsync(programCourseId,cancellationToken)) return Results.Forbid();
        var clo = await service.CreateCloAsync(
            versionId,
            programCourseId,
            request,
            cancellationToken);
        return Results.CreatedAtRoute(
            "GetCurriculumClo",
            new { versionId, programCourseId, cloId = clo.Id },
            clo);
    }

    private static async Task<IResult> UpdateCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        UpdateCloRequest request,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanEditCourseOutcomesAsync(programCourseId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.UpdateCloAsync(
            versionId,
            programCourseId,
            cloId,
            request,
            cancellationToken));
    }

    private static async Task<IResult> DeleteCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanEditCourseOutcomesAsync(programCourseId,cancellationToken)) return Results.Forbid();
        await service.DeleteCloAsync(
            versionId,
            programCourseId,
            cloId,
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetCoursePloMappingsAsync(
        long versionId,
        long programCourseId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadCourseAsync(programCourseId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetCoursePloMappingsAsync(
            versionId,
            programCourseId,
            cancellationToken));
    }

    private static async Task<IResult> CreateCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        CreateCoursePloMappingRequest request,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        var mapping = await service.CreateCoursePloMappingAsync(
            versionId,
            programCourseId,
            request,
            cancellationToken);
        return Results.Created(
            $"/api/curriculum/program-versions/{versionId}/courses/{programCourseId}/plo-mappings/{mapping.Id}",
            mapping);
    }

    private static async Task<IResult> UpdateCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        UpdateCoursePloMappingRequest request,
        CurriculumService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.UpdateCoursePloMappingAsync(
            versionId,
            programCourseId,
            mappingId,
            request,
            cancellationToken));

    private static async Task<IResult> DeleteCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteCoursePloMappingAsync(
            versionId,
            programCourseId,
            mappingId,
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetCloPloMappingsAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadCourseAsync(programCourseId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetCloPloMappingsAsync(
            versionId,
            programCourseId,
            cloId,
            cancellationToken));
    }

    private static async Task<IResult> CreateCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CreateCloPloMappingRequest request,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanEditCourseOutcomesAsync(programCourseId,cancellationToken)) return Results.Forbid();
        var mapping = await service.CreateCloPloMappingAsync(
            versionId,
            programCourseId,
            cloId,
            request,
            cancellationToken);
        return Results.Created(
            $"/api/curriculum/program-versions/{versionId}/courses/{programCourseId}/clos/{cloId}/plo-mappings/{mapping.CoursePloId}",
            mapping);
    }

    private static async Task<IResult> DeleteCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        long coursePloId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanEditCourseOutcomesAsync(programCourseId,cancellationToken)) return Results.Forbid();
        await service.DeleteCloPloMappingAsync(
            versionId,
            programCourseId,
            cloId,
            coursePloId,
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetPloCreditCheckAsync(
        long versionId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadVersionAsync(versionId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetPloCreditCheckAsync(versionId, cancellationToken));
    }

    private static async Task<IResult> GetPloBalanceAsync(
        long versionId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadVersionAsync(versionId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetPloBalanceAsync(versionId, cancellationToken));
    }
}
