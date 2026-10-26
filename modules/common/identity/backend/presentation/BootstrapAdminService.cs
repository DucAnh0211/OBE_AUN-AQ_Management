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

public sealed class BootstrapAdminService(
    IServiceScopeFactory scopeFactory,IOptions<BootstrapAdminOptions> options,ILogger<BootstrapAdminService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope=scopeFactory.CreateAsyncScope();
        var repository=scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
        if(await repository.CountActiveAdminsAsync(ct)>0) return;
        var settings=options.Value;
        if(string.IsNullOrWhiteSpace(settings.Email)||string.IsNullOrWhiteSpace(settings.Password))
            throw new InvalidOperationException("Chưa có admin. Hãy cấu hình BootstrapAdmin:Email và BootstrapAdmin:Password.");
        IdentityService.ValidatePassword(settings.Password);
        var hasher=scope.ServiceProvider.GetRequiredService<IPasswordHasher<SecurityUser>>();
        var email=settings.Email.Trim().ToLowerInvariant();
        var placeholder=new SecurityUser(0,email,email,settings.FullName,string.Empty,SystemRoles.Admin,"active",true,1,0,null,null,DateTime.UtcNow,DateTime.UtcNow);
        await repository.CreateUserAsync(email,email,settings.FullName,hasher.HashPassword(placeholder,settings.Password),SystemRoles.Admin,true,ct);
        logger.LogInformation("Đã khởi tạo tài khoản admin đầu tiên {Email}.",email);
    }
    public Task StopAsync(CancellationToken ct)=>Task.CompletedTask;
}
