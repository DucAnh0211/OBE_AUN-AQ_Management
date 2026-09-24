using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Accreditation.Presentation;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Accreditation;

public static class AccreditationModule
{
    public static readonly ModuleDescriptor Descriptor = new(
        "accreditation", "Kiểm định AUN-QA", "SV4 - Đức Anh", "/api/accreditation",
        "Quản lý phiên bản khung AUN-QA, tiêu chí và yêu cầu đánh giá.");

    public static IServiceCollection AddAccreditationModule(this IServiceCollection services)
    {
        services.AddScoped<AccreditationService>();
        services.AddExceptionHandler<AccreditationExceptionHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapAccreditationModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(Descriptor.ApiPrefix).WithTags("Accreditation");
        group.MapGet("/health", () => Results.Ok(new ModuleHealth(Descriptor.Key,"ready",DateTimeOffset.UtcNow)));
        group.MapGet("/capabilities", () => Results.Ok(new[] { "aun-framework-versioning", "aun-criteria", "aun-requirements", "framework-import" }));
        group.MapGet("/frameworks", async (AccreditationService service, CancellationToken ct, string? status=null, string? q=null)
            => Results.Ok(await service.GetFrameworksAsync(status,q,ct)));
        group.MapGet("/frameworks/{id:long:min(1)}", async (long id, AccreditationService service, CancellationToken ct)
            => Results.Ok(await service.GetFrameworkAsync(id,ct))).WithName("GetAunFramework");
        group.MapGet("/frameworks/{id:long:min(1)}/tree", async (long id, AccreditationService service, CancellationToken ct, string? language=null)
            => Results.Ok(await service.GetTreeAsync(id,language,ct)));
        group.MapPost("/frameworks", async (CreateFrameworkRequest request, AccreditationService service, CancellationToken ct) =>
        {
            var id=await service.CreateFrameworkAsync(request,ct);
            return Results.CreatedAtRoute("GetAunFramework",new{id},await service.GetFrameworkAsync(id,ct));
        });
        group.MapPut("/frameworks/{id:long:min(1)}", async (long id,UpdateFrameworkRequest request,AccreditationService service,CancellationToken ct)
            => {await service.UpdateFrameworkAsync(id,request,ct);return Results.Ok(await service.GetFrameworkAsync(id,ct));});
        group.MapDelete("/frameworks/{id:long:min(1)}", async (long id, AccreditationService service, CancellationToken ct)
            => { await service.DeleteFrameworkAsync(id,ct); return Results.NoContent(); });
        group.MapPost("/frameworks/{id:long:min(1)}/criteria", async (long id, CriterionRequest request, AccreditationService service, CancellationToken ct)
            => Results.Created($"{Descriptor.ApiPrefix}/frameworks/{id}/tree",new{id=await service.AddCriterionAsync(id,request,ct)}));
        group.MapPut("/frameworks/{id:long:min(1)}/criteria/{criterionId:long:min(1)}", async (long id,long criterionId,CriterionRequest request,AccreditationService service,CancellationToken ct)
            => {await service.UpdateCriterionAsync(id,criterionId,request,ct);return Results.NoContent();});
        group.MapDelete("/frameworks/{id:long:min(1)}/criteria/{criterionId:long:min(1)}", async (long id,long criterionId,AccreditationService service,CancellationToken ct)
            => {await service.DeleteCriterionAsync(id,criterionId,ct);return Results.NoContent();});
        group.MapPost("/frameworks/{id:long:min(1)}/criteria/{criterionId:long:min(1)}/requirements", async (long id,long criterionId,RequirementRequest request,AccreditationService service,CancellationToken ct)
            => Results.Created($"{Descriptor.ApiPrefix}/frameworks/{id}/tree",new{id=await service.AddRequirementAsync(id,criterionId,request,ct)}));
        group.MapPut("/frameworks/{id:long:min(1)}/requirements/{requirementId:long:min(1)}", async (long id,long requirementId,RequirementRequest request,AccreditationService service,CancellationToken ct)
            => {await service.UpdateRequirementAsync(id,requirementId,request,ct);return Results.NoContent();});
        group.MapDelete("/frameworks/{id:long:min(1)}/requirements/{requirementId:long:min(1)}", async (long id,long requirementId,AccreditationService service,CancellationToken ct)
            => {await service.DeleteRequirementAsync(id,requirementId,ct);return Results.NoContent();});
        group.MapPost("/framework-imports/preview", (FrameworkImportRequest request, AccreditationService service)
            => Results.Ok(service.PreviewImport(request)));
        group.MapPost("/framework-imports", async (FrameworkImportRequest request, AccreditationService service, CancellationToken ct) =>
        {
            var id=await service.ImportAsync(request,ct);
            return Results.CreatedAtRoute("GetAunFramework",new{id},await service.GetFrameworkAsync(id,ct));
        });
        group.MapPost("/frameworks/{id:long:min(1)}/publish", async (long id,AccreditationService service,CancellationToken ct)
            => {await service.PublishAsync(id,ct);return Results.Ok(await service.GetFrameworkAsync(id,ct));});
        group.MapPost("/frameworks/{id:long:min(1)}/retire", async (long id,AccreditationService service,CancellationToken ct)
            => {await service.RetireAsync(id,ct);return Results.Ok(await service.GetFrameworkAsync(id,ct));});
        group.MapPut("/frameworks/{id:long:min(1)}/criteria/{criterionId:long:min(1)}/translations/{language}", async (long id,long criterionId,string language,TranslationText request,AccreditationService service,CancellationToken ct)
            => {await service.UpsertCriterionTranslationAsync(id,criterionId,language,request,ct);return Results.NoContent();});
        group.MapPut("/frameworks/{id:long:min(1)}/requirements/{requirementId:long:min(1)}/translations/{language}", async (long id,long requirementId,string language,TranslationText request,AccreditationService service,CancellationToken ct)
            => {await service.UpsertRequirementTranslationAsync(id,requirementId,language,request,ct);return Results.NoContent();});
        return endpoints;
    }
}
