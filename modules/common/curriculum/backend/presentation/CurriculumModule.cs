using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Infrastructure;
using ObeAunQa.Modules.Curriculum.Presentation;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Curriculum;

public static class CurriculumModule
{
    public static readonly ModuleDescriptor Descriptor = new(
        "curriculum",
        "Chương trình đào tạo",
        "SV4 + SV5",
        "/api/curriculum",
        "Quản lý CTĐT, học phần, PLO, CLO và các bảng ánh xạ.");

    public static IServiceCollection AddCurriculumModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Thiếu connection string 'ConnectionStrings:Postgres'.");
        }

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
        services.AddScoped<ICurriculumRepository, NpgsqlCurriculumRepository>();
        services.AddScoped<CurriculumService>();
        services.AddExceptionHandler<CurriculumExceptionHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapCurriculumModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(Descriptor.ApiPrefix).WithTags("Curriculum");

        group.MapGet("/health", () => Results.Ok(new ModuleHealth(
            Descriptor.Key,
            "ready",
            DateTimeOffset.UtcNow)));

        group.MapGet("/capabilities", () => Results.Ok(new[]
        {
            "program-versions",
            "courses",
            "plos",
            "clos",
            "curriculum-mapping"
        }));

        group.MapGet("/programs", GetProgramsAsync);
        group.MapPost("/programs", CreateProgramAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/programs/{programId:long:min(1)}", GetProgramAsync)
            .WithName("GetCurriculumProgram");
        group.MapPut("/programs/{programId:long:min(1)}", UpdateProgramAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapDelete("/programs/{programId:long:min(1)}", ArchiveProgramAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/programs/{programId:long:min(1)}/versions", GetProgramVersionsAsync);
        group.MapPost("/programs/{programId:long:min(1)}/versions", CreateProgramVersionAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet("/program-versions/{versionId:long:min(1)}", GetProgramVersionAsync)
            .WithName("GetCurriculumProgramVersion");
        group.MapPut("/program-versions/{versionId:long:min(1)}", UpdateProgramVersionAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPost("/program-versions/{versionId:long:min(1)}/publish", PublishProgramVersionAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapDelete("/program-versions/{versionId:long:min(1)}", ArchiveProgramVersionAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/program-versions/{versionId:long:min(1)}/courses", GetProgramCoursesAsync);
        group.MapPost("/program-versions/{versionId:long:min(1)}/courses", CreateProgramCourseAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapGet(
                "/program-versions/{versionId:long:min(1)}/courses/{programCourseId:long:min(1)}",
                GetProgramCourseAsync)
            .WithName("GetCurriculumProgramCourse");
        group.MapPut(
            "/program-versions/{versionId:long:min(1)}/courses/{programCourseId:long:min(1)}",
            UpdateProgramCourseAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapDelete(
            "/program-versions/{versionId:long:min(1)}/courses/{programCourseId:long:min(1)}",
            ArchiveProgramCourseAsync).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapOutcomeEndpoints();

        return endpoints;
    }

    private static async Task<IResult> GetProgramsAsync(
        CurriculumService service,
        ICurrentUser currentUser,
        IAccessControlService access,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery(Name = "q")] string? search = null,
        [FromQuery] bool includeArchived = false)
    {
        if (currentUser.Role == SystemRoles.Admin)
            return Results.Ok(await service.GetProgramsAsync(page,pageSize,search,includeArchived,cancellationToken));
        var allowed = (await access.GetReadableProgramIdsAsync(cancellationToken)).ToHashSet();
        var all = new List<ProgramResponse>();
        for (var fetchPage=1;;fetchPage++)
        {
            var batch=await service.GetProgramsAsync(fetchPage,100,search,false,cancellationToken);
            all.AddRange(batch.Items.Where(x=>allowed.Contains(x.Id)));
            if(fetchPage>=batch.TotalPages) break;
        }
        page=Math.Max(1,page); pageSize=Math.Clamp(pageSize,1,100);
        var items=all.Skip((page-1)*pageSize).Take(pageSize).ToList();
        return Results.Ok(PagedResult<ProgramResponse>.Create(items,page,pageSize,all.Count));
    }

    private static async Task<IResult> CreateProgramAsync(
        CreateProgramRequest request,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        var program = await service.CreateProgramAsync(request, cancellationToken);
        return Results.CreatedAtRoute(
            "GetCurriculumProgram",
            new { programId = program.Id },
            program);
    }

    private static async Task<IResult> GetProgramAsync(
        long programId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadProgramAsync(programId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetProgramAsync(programId, cancellationToken));
    }

    private static async Task<IResult> UpdateProgramAsync(
        long programId,
        UpdateProgramRequest request,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await service.UpdateProgramAsync(
            programId,
            request,
            cancellationToken));
    }

    private static async Task<IResult> ArchiveProgramAsync(
        long programId,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        await service.ArchiveProgramAsync(programId, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetProgramVersionsAsync(
        long programId,
        CurriculumService service,
        ICurrentUser currentUser,
        IAccessControlService access,
        CancellationToken cancellationToken,
        [FromQuery] bool includeArchived = false)
    {
        if (!await access.CanReadProgramAsync(programId,cancellationToken)) return Results.Forbid();
        var versions = await service.GetProgramVersionsAsync(
            programId,
            currentUser.Role == SystemRoles.Admin && includeArchived,
            cancellationToken);
        if (currentUser.Role == SystemRoles.Admin) return Results.Ok(versions);
        var readable = new List<ProgramVersionResponse>();
        foreach (var version in versions)
            if (await access.CanReadVersionAsync(version.Id,cancellationToken)) readable.Add(version);
        return Results.Ok(readable);
    }

    private static async Task<IResult> CreateProgramVersionAsync(
        long programId,
        CreateProgramVersionRequest request,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        var version = await service.CreateProgramVersionAsync(
            programId,
            request,
            cancellationToken);
        return Results.CreatedAtRoute(
            "GetCurriculumProgramVersion",
            new { versionId = version.Id },
            version);
    }

    private static async Task<IResult> GetProgramVersionAsync(
        long versionId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadVersionAsync(versionId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetProgramVersionAsync(versionId, cancellationToken));
    }

    private static async Task<IResult> UpdateProgramVersionAsync(
        long versionId,
        UpdateProgramVersionRequest request,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await service.UpdateProgramVersionAsync(
            versionId,
            request,
            cancellationToken));
    }

    private static async Task<IResult> PublishProgramVersionAsync(
        long versionId,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await service.PublishProgramVersionAsync(versionId, cancellationToken));
    }

    private static async Task<IResult> ArchiveProgramVersionAsync(
        long versionId,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        await service.ArchiveProgramVersionAsync(versionId, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetProgramCoursesAsync(
        long versionId,
        CurriculumService service,
        ICurrentUser currentUser,
        IAccessControlService access,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery(Name = "q")] string? search = null,
        [FromQuery] string? semester = null,
        [FromQuery] bool includeArchived = false)
    {
        if (!await access.CanReadVersionAsync(versionId,cancellationToken)) return Results.Forbid();
        if (currentUser.Role == SystemRoles.Admin || currentUser.Role == SystemRoles.Student)
            return Results.Ok(await service.GetProgramCoursesAsync(versionId,page,pageSize,search,semester,
                currentUser.Role == SystemRoles.Admin && includeArchived,cancellationToken));
        var allowed = (await access.GetReadableCourseIdsAsync(versionId,cancellationToken)).ToHashSet();
        var all = new List<ProgramCourseResponse>();
        for(var fetchPage=1;;fetchPage++)
        {
            var batch=await service.GetProgramCoursesAsync(versionId,fetchPage,100,search,semester,false,cancellationToken);
            all.AddRange(batch.Items.Where(x=>allowed.Contains(x.Id)));
            if(fetchPage>=batch.TotalPages) break;
        }
        page=Math.Max(1,page); pageSize=Math.Clamp(pageSize,1,100);
        var items=all.Skip((page-1)*pageSize).Take(pageSize).ToList();
        return Results.Ok(PagedResult<ProgramCourseResponse>.Create(items,page,pageSize,all.Count));
    }

    private static async Task<IResult> CreateProgramCourseAsync(
        long versionId,
        CreateProgramCourseRequest request,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        var course = await service.CreateProgramCourseAsync(
            versionId,
            request,
            cancellationToken);
        return Results.CreatedAtRoute(
            "GetCurriculumProgramCourse",
            new { versionId, programCourseId = course.Id },
            course);
    }

    private static async Task<IResult> GetProgramCourseAsync(
        long versionId,
        long programCourseId,
        CurriculumService service,
        IAccessControlService access,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadCourseAsync(programCourseId,cancellationToken)) return Results.Forbid();
        return Results.Ok(await service.GetProgramCourseAsync(
            versionId,
            programCourseId,
            cancellationToken));
    }

    private static async Task<IResult> UpdateProgramCourseAsync(
        long versionId,
        long programCourseId,
        UpdateProgramCourseRequest request,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await service.UpdateProgramCourseAsync(
            versionId,
            programCourseId,
            request,
            cancellationToken));
    }

    private static async Task<IResult> ArchiveProgramCourseAsync(
        long versionId,
        long programCourseId,
        CurriculumService service,
        CancellationToken cancellationToken)
    {
        await service.ArchiveProgramCourseAsync(
            versionId,
            programCourseId,
            cancellationToken);
        return Results.NoContent();
    }
}
