using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Accreditation;

public static class AccreditationModule
{
    public static readonly ModuleDescriptor Descriptor = new(
        "accreditation",
        "Kiểm định AUN-QA",
        "SV4 - Đức Anh",
        "/api/accreditation",
        "Quản lý tiêu chí AUN-QA, minh chứng, ingestion, truy xuất và gap analysis.");

    public static IServiceCollection AddAccreditationModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapAccreditationModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(Descriptor.ApiPrefix);

        group.MapGet("/health", () => Results.Ok(new ModuleHealth(
            Descriptor.Key,
            "ready",
            DateTimeOffset.UtcNow)));

        group.MapGet("/capabilities", () => Results.Ok(new[]
        {
            "aun-criteria",
            "evidence",
            "document-ingestion",
            "retrieval",
            "gap-analysis"
        }));

        return endpoints;
    }
}
