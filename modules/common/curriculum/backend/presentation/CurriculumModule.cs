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
        group.MapPost("/programs", CreateProgramAsync);
        group.MapGet("/programs/{programId:long:min(1)}", GetProgramAsync)
            .WithName("GetCurriculumProgram");
        group.MapPut("/programs/{programId:long:min(1)}", UpdateProgramAsync);
        group.MapDelete("/programs/{programId:long:min(1)}", ArchiveProgramAsync);

        group.MapGet("/programs/{programId:long:min(1)}/versions", GetProgramVersionsAsync);
        group.MapPost("/programs/{programId:long:min(1)}/versions", CreateProgramVersionAsync);
        group.MapGet("/program-versions/{versionId:long:min(1)}", GetProgramVersionAsync)
            .WithName("GetCurriculumProgramVersion");
        group.MapPut("/program-versions/{versionId:long:min(1)}", UpdateProgramVersionAsync);
        group.MapPost("/program-versions/{versionId:long:min(1)}/publish", PublishProgramVersionAsync);
        group.MapDelete("/program-versions/{versionId:long:min(1)}", ArchiveProgramVersionAsync);

        group.MapGet("/program-versions/{versionId:long:min(1)}/courses", GetProgramCoursesAsync);
        group.MapPost("/program-versions/{versionId:long:min(1)}/courses", CreateProgramCourseAsync);
        group.MapGet(
                "/program-versions/{versionId:long:min(1)}/courses/{programCourseId:long:min(1)}",
                GetProgramCourseAsync)
            .WithName("GetCurriculumProgramCourse");
        group.MapPut(
            "/program-versions/{versionId:long:min(1)}/courses/{programCourseId:long:min(1)}",
            UpdateProgramCourseAsync);
        group.MapDelete(
            "/program-versions/{versionId:long:min(1)}/courses/{programCourseId:long:min(1)}",
            ArchiveProgramCourseAsync);

        group.MapOutcomeEndpoints();

        return endpoints;
    }

    private static async Task<IResult> GetProgramsAsync(
        CurriculumService service,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery(Name = "q")] string? search = null,
        [FromQuery] bool includeArchived = false)
    {
        return Results.Ok(await service.GetProgramsAsync(
            page,
            pageSize,
            search,
            includeArchived,
            cancellationToken));
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
        CancellationToken cancellationToken)
    {
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
        CancellationToken cancellationToken,
        [FromQuery] bool includeArchived = false)
    {
        return Results.Ok(await service.GetProgramVersionsAsync(
            programId,
            includeArchived,
            cancellationToken));
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
        CancellationToken cancellationToken)
    {
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
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery(Name = "q")] string? search = null,
        [FromQuery] string? semester = null,
        [FromQuery] bool includeArchived = false)
    {
        return Results.Ok(await service.GetProgramCoursesAsync(
            versionId,
            page,
            pageSize,
            search,
            semester,
            includeArchived,
            cancellationToken));
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
        CancellationToken cancellationToken)
    {
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
