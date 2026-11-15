using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
namespace ObeAunQa.SharedKernel;
public sealed class SchoolAccessExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http,Exception exception,CancellationToken ct) {
        if(exception is not SchoolAccessException) return false;
        http.Response.StatusCode=403;
        await Results.Problem(statusCode:403,title:"Không có quyền truy cập",detail:exception.Message).ExecuteAsync(http);
        return true;
    }
}
