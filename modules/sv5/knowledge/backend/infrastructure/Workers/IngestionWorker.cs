using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Knowledge.Application;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class IngestionWorker(IServiceScopeFactory scopes, ILogger<IngestionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IKnowledgeRepository>();
                var job = await repository.ClaimAsync(stoppingToken);
                if (job is not null)
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    timeout.CancelAfter(TimeSpan.FromMinutes(10));
                    try { await scope.ServiceProvider.GetRequiredService<IngestionProcessor>().ProcessAsync(job, timeout.Token); }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                    catch (Exception ex)
                    {
                        var message = ex switch { KnowledgeException k => k.Message, DocumentException d => d.Message,
                            OperationCanceledException => "Xử lý vượt 10 phút. Hãy chia nhỏ tài liệu và thử lại.",
                            _ => "Xử lý thất bại. Kiểm tra dịch vụ lưu trữ/embedding và thử lại." };
                        // Provider error bodies and document content never enter logs or UI errors.
                        logger.LogWarning("Knowledge ingestion {DocumentId} failed ({ErrorType})", job.DocumentId, ex.GetType().Name);
                        using var failureScope = scopes.CreateScope();
                        await failureScope.ServiceProvider.GetRequiredService<IKnowledgeRepository>().FailAsync(job, message, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogWarning("Knowledge worker unavailable ({ErrorType}); check migrations and database", ex.GetType().Name); }
            try { await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
