using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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

    public static IServiceCollection AddCurriculumModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapCurriculumModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(Descriptor.ApiPrefix);

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

        return endpoints;
    }
}
