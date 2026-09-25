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

public static class IdentityModule
{
    public const string RefreshCookieName = "obe_refresh_token";

    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<BootstrapAdminOptions>(configuration.GetSection(BootstrapAdminOptions.SectionName));
        var jwt=configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if(Encoding.UTF8.GetByteCount(jwt.SigningKey)<32)
            throw new InvalidOperationException("Jwt:SigningKey phải có ít nhất 32 byte.");

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser,HttpCurrentUser>();
        services.AddScoped<IAccessControlService,AccessControlService>();
        services.AddScoped<IdentityRepository>();
        services.AddScoped<IAssignmentCloneService>(sp=>sp.GetRequiredService<IdentityRepository>());
        services.AddScoped<IdentityService>();
        services.AddSingleton<TokenService>();
        services.AddSingleton<IPasswordHasher<SecurityUser>,PasswordHasher<SecurityUser>>();
        services.AddHostedService<BootstrapAdminService>();
        services.AddExceptionHandler<IdentityExceptionHandler>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims=false;
                options.TokenValidationParameters=new TokenValidationParameters
                {
                    ValidateIssuer=true,ValidIssuer=jwt.Issuer,
                    ValidateAudience=true,ValidAudience=jwt.Audience,
                    ValidateIssuerSigningKey=true,
                    IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime=true,ClockSkew=TimeSpan.FromSeconds(30),
                    NameClaimType=ClaimTypes.Name,RoleClaimType=ClaimTypes.Role
                };
                options.Events=new JwtBearerEvents
                {
                    OnTokenValidated=async context =>
                    {
                        var idValue=context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var versionValue=context.Principal?.FindFirstValue("token_version");
                        if(!long.TryParse(idValue,out var id) || !int.TryParse(versionValue,out var version))
                        { context.Fail("Token claims are invalid."); return; }
                        var repository=context.HttpContext.RequestServices.GetRequiredService<IdentityRepository>();
                        if(!await repository.IsUserTokenValidAsync(id,version,context.HttpContext.RequestAborted))
                            context.Fail("User session is no longer valid.");
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().RequireClaim("must_change_password","false").Build())
            .AddPolicy(AuthorizationPolicies.Authenticated,p=>p.RequireAuthenticatedUser())
            .AddPolicy(AuthorizationPolicies.PasswordChanged,p=>p.RequireAuthenticatedUser().RequireClaim("must_change_password","false"))
            .AddPolicy(AuthorizationPolicies.AdminOnly,p=>p.RequireRole(SystemRoles.Admin).RequireClaim("must_change_password","false"))
            .AddPolicy(AuthorizationPolicies.AdminOrLecturer,p=>p.RequireRole(SystemRoles.Admin,SystemRoles.Lecturer).RequireClaim("must_change_password","false"));
        return services;
    }

    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder endpoints)
    {
        var auth=endpoints.MapGroup("/api/auth").WithTags("Authentication");
        auth.MapPost("/login",LoginAsync).AllowAnonymous();
        auth.MapPost("/refresh",RefreshAsync).AllowAnonymous();
        auth.MapPost("/logout",LogoutAsync).AllowAnonymous();
        auth.MapGet("/me",GetMeAsync).RequireAuthorization(AuthorizationPolicies.Authenticated);
        auth.MapPost("/change-password",ChangePasswordAsync).RequireAuthorization(AuthorizationPolicies.Authenticated);

        var admin=endpoints.MapGroup("/api/admin").WithTags("Administration")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        admin.MapGet("/users",async (IdentityService service,CancellationToken ct,int page=1,int pageSize=20,string? q=null)
            =>Results.Ok(await service.GetUsersAsync(page,pageSize,q,ct)));
        admin.MapPost("/users",CreateUserAsync);
        admin.MapPut("/users/{id:long:min(1)}",UpdateUserAsync);
        admin.MapPost("/users/{id:long:min(1)}/reset-password",ResetPasswordAsync);
        admin.MapGet("/users/{id:long:min(1)}/course-assignments",async(long id,IdentityRepository repository,CancellationToken ct)
            =>Results.Ok(await repository.GetCourseAssignmentsAsync(id,ct)));
        admin.MapPut("/users/{id:long:min(1)}/course-assignments",ReplaceCourseAssignmentsAsync);
        admin.MapGet("/users/{id:long:min(1)}/program-assignments",async(long id,IdentityRepository repository,CancellationToken ct)
            =>Results.Ok(await repository.GetProgramAssignmentsAsync(id,ct)));
        admin.MapPut("/users/{id:long:min(1)}/program-assignments",ReplaceProgramAssignmentsAsync);
        admin.MapGet("/audit-logs",async(IdentityRepository repository,CancellationToken ct,int page=1,int pageSize=50)
            =>Results.Ok(await repository.GetAuditLogsAsync(Math.Max(1,page),Math.Clamp(pageSize,1,100),ct)));
        return endpoints;
    }

    private static async Task<IResult> LoginAsync(LoginRequest request,IdentityService service,HttpContext http,CancellationToken ct)
    { var session=await service.LoginAsync(request,Ip(http),ct); SetRefreshCookie(http,session); return Results.Ok(session.Response); }
    private static async Task<IResult> RefreshAsync(IdentityService service,HttpContext http,CancellationToken ct)
    { if(!http.Request.Cookies.TryGetValue(RefreshCookieName,out var raw)) throw new IdentityApiException(401,"refresh_token_missing","Không có phiên đăng nhập."); var session=await service.RefreshAsync(raw,Ip(http),ct); SetRefreshCookie(http,session); return Results.Ok(session.Response); }
    private static async Task<IResult> LogoutAsync(IdentityService service,HttpContext http,CancellationToken ct)
    { http.Request.Cookies.TryGetValue(RefreshCookieName,out var raw); await service.LogoutAsync(raw,Ip(http),ct); DeleteRefreshCookie(http); return Results.NoContent(); }
    private static async Task<IResult> GetMeAsync(IdentityService service,CancellationToken ct)
    { var user=await service.GetCurrentProfileAsync(ct); return Results.Ok(new{user,permissions=Permissions(user.Role)}); }
    private static async Task<IResult> ChangePasswordAsync(ChangePasswordRequest request,IdentityService service,HttpContext http,CancellationToken ct)
    { var session=await service.ChangePasswordAsync(request,Ip(http),ct); SetRefreshCookie(http,session); return Results.Ok(session.Response); }
    private static async Task<IResult> CreateUserAsync(CreateUserRequest request,IdentityService service,HttpContext http,CancellationToken ct)
    =>Results.Created("/api/admin/users",await service.CreateUserAsync(request,Ip(http),ct));
    private static async Task<IResult> UpdateUserAsync(long id,UpdateUserRequest request,IdentityService service,HttpContext http,CancellationToken ct)
    =>Results.Ok(await service.UpdateUserAsync(id,request,Ip(http),ct));
    private static async Task<IResult> ResetPasswordAsync(long id,ResetPasswordRequest request,IdentityService service,HttpContext http,CancellationToken ct)
    { await service.ResetPasswordAsync(id,request,Ip(http),ct); return Results.NoContent(); }
    private static async Task<IResult> ReplaceCourseAssignmentsAsync(long id,AssignmentRequest request,IdentityService service,HttpContext http,CancellationToken ct)
    { await service.ReplaceCourseAssignmentsAsync(id,request.Ids,Ip(http),ct); return Results.NoContent(); }
    private static async Task<IResult> ReplaceProgramAssignmentsAsync(long id,AssignmentRequest request,IdentityService service,HttpContext http,CancellationToken ct)
    { await service.ReplaceProgramAssignmentsAsync(id,request.Ids,Ip(http),ct); return Results.NoContent(); }

    private static void SetRefreshCookie(HttpContext http,IssuedSession session) => http.Response.Cookies.Append(
        RefreshCookieName,session.RefreshToken,new CookieOptions{HttpOnly=true,Secure=!http.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment(),SameSite=SameSiteMode.Strict,Expires=session.RefreshExpiresAt,Path="/api/auth"});
    private static void DeleteRefreshCookie(HttpContext http) => http.Response.Cookies.Delete(RefreshCookieName,new CookieOptions{Path="/api/auth"});
    private static string? Ip(HttpContext http)=>http.Connection.RemoteIpAddress?.ToString();
    private static string[] Permissions(string role)=>role switch
    { SystemRoles.Admin=>["users.manage","curriculum.manage","accreditation.manage"], SystemRoles.Lecturer=>["assigned_courses.read","assigned_clos.manage","accreditation.read"], SystemRoles.Student=>["assigned_program.read"], _=>[] };
}

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

public sealed class BootstrapAdminService(
    IServiceScopeFactory scopeFactory,IOptions<BootstrapAdminOptions> options,ILogger<BootstrapAdminService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope=scopeFactory.CreateAsyncScope();
        var repository=scope.ServiceProvider.GetRequiredService<IdentityRepository>();
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
