using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Reporting.Application;
using System.Threading.RateLimiting;
using ObeAunQa.Modules.Reporting.Infrastructure;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Reporting;

public static class ReportingModule
{
    public static readonly ModuleDescriptor Descriptor = new(
        "reporting",
        "Báo cáo và trợ lý",
        "SV5 - Chí Hoàng",
        "/api/reporting",
        "Pipeline minh chứng và retrieval dùng chung, chat có trích dẫn, báo cáo và xuất tài liệu.");

    public static IServiceCollection AddReportingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ReportingAccessOptions>(configuration.GetSection("Reporting:Access"));
        services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, ReportingAuthenticationHandler>(ReportingAuthenticationHandler.SchemeName, _ => { });
        services.AddTransient<IClaimsTransformation, ReportingClaimsTransformation>();
        services.AddAuthorization(options => options.AddPolicy("Reporting", policy =>
            policy.AddAuthenticationSchemes("Bearer", ReportingAuthenticationHandler.SchemeName)
                .RequireAuthenticatedUser().RequireClaim("reporting:access", "true")));
        services.AddRateLimiter(options => {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("report-work", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.Identity?.Name ?? "anonymous", _ => new FixedWindowRateLimiterOptions {
                    PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
                }));
        });
        services.AddDbContext<ReportingDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Postgres"),
            postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "reporting")));
        services.AddScoped<IReportRepository, EfReportRepository>();
        services.AddScoped<ReportService>();
        services.AddSingleton<IReportExporter, ReportExporter>();
        services.AddExceptionHandler<ReportingExceptionHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapReportingModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(Descriptor.ApiPrefix)
            .RequireAuthorization("Reporting");

        group.MapGet("/health", () => Results.Ok(new ModuleHealth(
            Descriptor.Key,
            "ready",
            DateTimeOffset.UtcNow)));

        group.MapGet("/capabilities", () => Results.Ok(new[]
        {
            "document-ingestion",
            "retrieval",
            "course-reports",
            "clo-reports",
            "plo-reports",
            "chat",
            "citations",
            "sar-reports",
            "export"
        }));

        group.MapReports();

        return endpoints;
    }
}
