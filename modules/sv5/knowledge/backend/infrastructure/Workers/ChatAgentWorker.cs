using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Accreditation.Application;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class ChatAgentWorker(IServiceScopeFactory scopes, ILogger<ChatAgentWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IChatThreadRepository>();
                var run = await repository.ClaimAsync(stoppingToken);
                if (run is not null)
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken); timeout.CancelAfter(TimeSpan.FromMinutes(8));
                    try
                    {
                        var history = await repository.HistoryAsync(run, timeout.Token);
                        var answer = await scope.ServiceProvider.GetRequiredService<IChatAgent>().RunAsync(new(run.ThreadId, run.Id, run.Lease!.Value, run.Question, history), timeout.Token);
                        await scope.ServiceProvider.GetRequiredService<KnowledgeChatService>().ValidateAnswerAsync(answer, timeout.Token,
                            await repository.OwnerAsync(run.ThreadId, timeout.Token));
                        await repository.CompleteAsync(run, answer, timeout.Token);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                    catch (Exception error)
                    {
                        var message = error switch { KnowledgeException k => k.Message, DocumentException d => d.Message,
                            OperationCanceledException => "Tôi chưa xử lý xong trong thời gian cho phép. Bạn có thể bấm thử lại nhé.",
                            _ => "Kết nối với trợ lý bị gián đoạn. Bạn thử lại nhé; tin nhắn đã được lưu." };
                        logger.LogWarning("Chat run {RunId} failed ({ErrorType})", run.Id, error.GetType().Name);
                        using var failure = scopes.CreateScope();
                        await failure.ServiceProvider.GetRequiredService<IChatThreadRepository>().FailAsync(run, message, stoppingToken);
                    }
                    continue;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception error) { logger.LogWarning("Chat queue unavailable ({ErrorType})", error.GetType().Name); }
            try { await Task.Delay(1000, stoppingToken); } catch (OperationCanceledException) { break; }
        }
    }
}
