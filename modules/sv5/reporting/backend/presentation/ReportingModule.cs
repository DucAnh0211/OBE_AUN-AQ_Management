using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Reporting;

public static class ReportingModule
{
    public static readonly ModuleDescriptor Descriptor = new(
        "reporting",
        "Báo cáo và trợ lý",
        "SV5 - Chí Hoàng",
        "/api/reporting",
        "Chat có trích dẫn, tổng hợp báo cáo và xuất tài liệu.");

    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapReportingModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(Descriptor.ApiPrefix);

        group.MapGet("/health", () => Results.Ok(new ModuleHealth(
            Descriptor.Key,
            "ready",
            DateTimeOffset.UtcNow)));

        group.MapGet("/capabilities", () => Results.Ok(new[]
        {
            "chat",
            "citations",
            "sar-reports",
            "export"
        }));

        return endpoints;
    }
}
