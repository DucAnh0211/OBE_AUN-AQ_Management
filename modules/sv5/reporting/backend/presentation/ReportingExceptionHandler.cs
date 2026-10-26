using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using ObeAunQa.Modules.Reporting.Application;

namespace ObeAunQa.Modules.Reporting;

public sealed class ReportingExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not ReportException error) return false;
        context.Response.StatusCode = error.StatusCode;
        await Results.Problem(statusCode: error.StatusCode, title: "Không thể xử lý báo cáo", detail: error.Message).ExecuteAsync(context);
        return true;
    }
}
