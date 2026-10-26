using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Identity;

public sealed class IdentityExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context,Exception exception,CancellationToken ct)
    {
        if(exception is not IdentityApiException known) return false;
        context.Response.StatusCode=known.StatusCode;
        await Results.Problem(statusCode:known.StatusCode,title:"Yêu cầu không hợp lệ",detail:known.Message,
            extensions:new Dictionary<string,object?>{{"code",known.Code}}).ExecuteAsync(context);
        return true;
    }
}
