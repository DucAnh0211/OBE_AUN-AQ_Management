using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using ObeAunQa.Modules.Knowledge.Application;
namespace ObeAunQa.Modules.Knowledge;
public sealed class KnowledgeExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not KnowledgeException e) return false;
        await Results.Problem(statusCode: e.StatusCode, title: "Knowledge Base", detail: e.Message).ExecuteAsync(context);
        return true;
    }
}
