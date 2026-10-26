using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

internal sealed class ReportingClaimsTransformation(IProgramRepository programs, IHttpContextAccessor accessor) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (accessor.HttpContext?.Request.Path.StartsWithSegments("/api/reporting") != true
            || principal.Identity?.IsAuthenticated != true || !principal.IsInRole(SystemRoles.Admin)
            || !principal.HasClaim("must_change_password", "false") || !principal.HasClaim(c => c.Type == "token_version")
            || principal.HasClaim("reporting:access", "true")) return principal;

        var result = principal.Clone();
        var identity = new ClaimsIdentity();
        identity.AddClaim(new("reporting:access", "true"));
        identity.AddClaim(new("reporting:generate", "true"));
        for (var page=1;;page++)
        {
            var rows=await programs.GetProgramsAsync(page,100,null,true,accessor.HttpContext.RequestAborted);
            foreach(var program in rows.Items) identity.AddClaim(new("reporting:program",program.Id.ToString()));
            if(rows.Items.Count<100) break;
        }
        result.AddIdentity(identity);
        return result;
    }
}
