using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObeAunQa.Modules.Accreditation.Application;

namespace ObeAunQa.Modules.Accreditation.Presentation;

internal sealed class AccreditationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        ProblemDetails? problem = exception switch
        {
            AccreditationValidationException validation => new HttpValidationProblemDetails(validation.Errors.ToDictionary(x=>x.Key,x=>x.Value))
            { Status=400, Title="Dữ liệu AUN-QA không hợp lệ", Detail=validation.Message, Type="https://httpstatuses.com/400" },
            AccreditationNotFoundException notFound => new ProblemDetails
            { Status=404, Title="Không tìm thấy dữ liệu AUN-QA", Detail=notFound.Message, Type="https://httpstatuses.com/404" },
            AccreditationConflictException conflict => new ProblemDetails
            { Status=409, Title="Xung đột dữ liệu AUN-QA", Detail=conflict.Message, Type="https://httpstatuses.com/409", Extensions={ ["code"]=conflict.Code } },
            _ => null
        };
        if (problem is null) return false;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}
