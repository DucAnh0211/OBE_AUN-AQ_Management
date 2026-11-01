using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Knowledge.Infrastructure;
namespace ObeAunQa.Modules.Knowledge;

public static class KnowledgeModule
{
    public static IServiceCollection AddKnowledgeModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KnowledgeDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("Postgres"), p => p.MigrationsHistoryTable("__EFMigrationsHistory", "knowledge")));
        services.Configure<KnowledgeAiOptions>(o => {
            o.Url = configuration["AI_SERVICE_URL"] ?? "http://localhost:8000";
            o.SharedKey = configuration["AI_SERVICE_KEY"] ?? "";
        });
        services.AddHttpClient<IKnowledgePipeline, PythonKnowledgePipeline>(http => http.Timeout = TimeSpan.FromMinutes(10));
        services.AddScoped<IKnowledgeRepository, EfKnowledgeRepository>();
        services.AddScoped<IKnowledgeSearchRepository, EfKnowledgeSearchRepository>();
        services.AddHttpClient<IKnowledgeChatPipeline, PythonKnowledgeChatPipeline>(http => http.Timeout = TimeSpan.FromSeconds(140));
        services.AddScoped<KnowledgeChatService>();
        services.AddScoped<ICurriculumChatSource, CurriculumChatSource>();
        services.AddScoped<IChatThreadRepository, EfChatThreadRepository>();
        services.AddHttpClient<IChatAgent, PythonChatAgent>(http => http.Timeout = TimeSpan.FromMinutes(8));
        services.AddHostedService<ChatAgentWorker>();
        services.AddScoped<IngestionProcessor>();
        services.AddHostedService<IngestionWorker>();
        services.AddExceptionHandler<KnowledgeExceptionHandler>();
        return services;
    }
    public static IEndpointRouteBuilder MapKnowledgeModule(this IEndpointRouteBuilder endpoints)
    {
        KnowledgeEndpoints.Map(endpoints); KnowledgeChatEndpoints.Map(endpoints); ChatAgentInternalEndpoints.Map(endpoints); return endpoints;
    }
}
