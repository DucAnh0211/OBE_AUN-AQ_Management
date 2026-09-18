using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Curriculum.Presentation;

internal sealed class CurriculumExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            CurriculumValidationException validation => CreateValidationProblem(validation),
            CurriculumNotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Không tìm thấy dữ liệu",
                Detail = notFound.Message,
                Type = "https://httpstatuses.com/404"
            },
            CurriculumConflictException conflict => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Xung đột dữ liệu",
                Detail = conflict.Message,
                Type = "https://httpstatuses.com/409",
                Extensions = { ["code"] = conflict.Code }
            },
            _ => null
        };

        if (problem is null)
        {
            return false;
        }

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static HttpValidationProblemDetails CreateValidationProblem(
        CurriculumValidationException exception)
    {
        return new HttpValidationProblemDetails(
            exception.Errors.ToDictionary(pair => pair.Key, pair => pair.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Dữ liệu không hợp lệ",
            Detail = exception.Message,
            Type = "https://httpstatuses.com/400"
        };
    }
}
