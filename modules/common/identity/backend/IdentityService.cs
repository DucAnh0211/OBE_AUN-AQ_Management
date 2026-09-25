using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Identity;

public sealed class TokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions settings = options.Value;

    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(SecurityUser user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(settings.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Email,user.Email),
            new Claim(ClaimTypes.Name,user.FullName),
            new Claim(ClaimTypes.Role,user.Role),
            new Claim("token_version",user.TokenVersion.ToString()),
            new Claim("must_change_password",user.MustChangePassword ? "true" : "false")
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(settings.Issuer,settings.Audience,claims,
            expires:expiresAt.UtcDateTime,signingCredentials:credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token),expiresAt);
    }

    public (Guid Id,string Raw,string Hash,DateTimeOffset ExpiresAt) CreateRefreshToken()
    {
        var raw=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (Guid.NewGuid(),raw,Hash(raw),DateTimeOffset.UtcNow.AddDays(settings.RefreshTokenDays));
    }

    public static string Hash(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
}

public sealed record IssuedSession(LoginResponse Response,string RefreshToken,DateTimeOffset RefreshExpiresAt);

public sealed class IdentityService(
    IdentityRepository repository,
    TokenService tokens,
    IPasswordHasher<SecurityUser> passwordHasher,
    ICurrentUser currentUser)
{
    public async Task<IssuedSession> LoginAsync(LoginRequest request,string? ip,CancellationToken ct)
    {
        var email=NormalizeEmail(request.Email);
        var user=await repository.FindByEmailAsync(email,ct);
        if(user is null || user.Status!="active") throw InvalidCredentials();
        if(user.LockedUntil>DateTime.UtcNow)
            throw new IdentityApiException(423,"account_locked","Tài khoản đang tạm khóa. Vui lòng thử lại sau.");
        var result=passwordHasher.VerifyHashedPassword(user,user.PasswordHash,request.Password??string.Empty);
        if(result==PasswordVerificationResult.Failed)
        {
            var attempts=user.FailedLoginCount+1;
            await repository.RecordFailedLoginAsync(user.Id,attempts>=5,ct);
            await repository.WriteAuditAsync(user.Id,"login_failed","user",user.Id.ToString(),new{attempts},ip,ct);
            throw InvalidCredentials();
        }
        if(result==PasswordVerificationResult.SuccessRehashNeeded)
            await repository.SetPasswordAsync(user.Id,passwordHasher.HashPassword(user,request.Password!),user.MustChangePassword,ct);
        await repository.RecordSuccessfulLoginAsync(user.Id,ct);
        user=(await repository.FindByIdAsync(user.Id,ct))!;
        var session=await IssueSessionAsync(user,ip,ct);
        await repository.WriteAuditAsync(user.Id,"login_succeeded","user",user.Id.ToString(),null,ip,ct);
        return session;
    }

    public async Task<IssuedSession> RefreshAsync(string rawToken,string? ip,CancellationToken ct)
    {
        var stored=await repository.FindRefreshTokenAsync(TokenService.Hash(rawToken),ct)
            ?? throw new IdentityApiException(401,"invalid_refresh_token","Phiên đăng nhập không hợp lệ.");
        if(stored.RevokedAt is not null)
        {
            await repository.RevokeAllSessionsAsync(stored.UserId,ct);
            throw new IdentityApiException(401,"refresh_token_reused","Phiên đăng nhập đã bị thu hồi.");
        }
        if(stored.ExpiresAt<=DateTime.UtcNow)
            throw new IdentityApiException(401,"refresh_token_expired","Phiên đăng nhập đã hết hạn.");
        var user=await repository.FindByIdAsync(stored.UserId,ct);
        if(user is null || user.Status!="active")
            throw new IdentityApiException(401,"account_unavailable","Tài khoản không còn hoạt động.");
        var refresh=tokens.CreateRefreshToken();
        await repository.SaveRefreshTokenAsync(refresh.Id,user.Id,refresh.Hash,refresh.ExpiresAt,ip,ct);
        await repository.RotateRefreshTokenAsync(stored.Id,refresh.Id,ip,ct);
        var access=tokens.CreateAccessToken(user);
        return new IssuedSession(new LoginResponse(access.Token,access.ExpiresAt,ToProfile(user)),refresh.Raw,refresh.ExpiresAt);
    }

    public async Task LogoutAsync(string? rawToken,string? ip,CancellationToken ct)
    {
        if(!string.IsNullOrWhiteSpace(rawToken))
            await repository.RevokeRefreshTokenAsync(TokenService.Hash(rawToken),ip,ct);
        if(currentUser.IsAuthenticated)
            await repository.WriteAuditAsync(currentUser.UserId,"logout","user",currentUser.UserId.ToString(),null,ip,ct);
    }

    public async Task<IssuedSession> ChangePasswordAsync(ChangePasswordRequest request,string? ip,CancellationToken ct)
    {
        var user=await RequireUserAsync(currentUser.UserId,ct);
        if(passwordHasher.VerifyHashedPassword(user,user.PasswordHash,request.CurrentPassword??string.Empty)==PasswordVerificationResult.Failed)
            throw new IdentityApiException(400,"current_password_invalid","Mật khẩu hiện tại không đúng.");
        ValidatePassword(request.NewPassword);
        await repository.SetPasswordAsync(user.Id,passwordHasher.HashPassword(user,request.NewPassword!),false,ct);
        await repository.WriteAuditAsync(user.Id,"password_changed","user",user.Id.ToString(),null,ip,ct);
        user=(await repository.FindByIdAsync(user.Id,ct))!;
        return await IssueSessionAsync(user,ip,ct);
    }

    public async Task<UserProfile> GetCurrentProfileAsync(CancellationToken ct) =>
        ToProfile(await RequireUserAsync(currentUser.UserId,ct));

    public Task<PagedUsers> GetUsersAsync(int page,int pageSize,string? search,CancellationToken ct) =>
        repository.GetUsersAsync(NormalizePage(page),NormalizePageSize(pageSize),Clean(search),ct);

    public async Task<UserProfile> CreateUserAsync(CreateUserRequest request,string? ip,CancellationToken ct)
    {
        var email=NormalizeEmail(request.Email);
        var name=Required(request.FullName,"fullName",150);
        var role=ValidateRole(request.Role);
        ValidatePassword(request.TemporaryPassword);
        var placeholder=new SecurityUser(0,email,email,name,string.Empty,role,"active",true,1,0,null,null,DateTime.UtcNow,DateTime.UtcNow);
        var created=await repository.CreateUserAsync(email,email,name,passwordHasher.HashPassword(placeholder,request.TemporaryPassword!),role,true,ct);
        await repository.WriteAuditAsync(currentUser.UserId,"user_created","user",created.Id.ToString(),new{created.Email,created.Role},ip,ct);
        return ToProfile(created);
    }

    public async Task<UserProfile> UpdateUserAsync(long id,UpdateUserRequest request,string? ip,CancellationToken ct)
    {
        var existing=await RequireUserAsync(id,ct);
        var role=ValidateRole(request.Role);
        var status=ValidateStatus(request.Status);
        if(id==currentUser.UserId && (role!=SystemRoles.Admin || status!="active"))
            throw new IdentityApiException(409,"cannot_restrict_self","Không thể tự khóa hoặc hạ quyền tài khoản đang đăng nhập.");
        if(existing.Role==SystemRoles.Admin && existing.Status=="active" && (role!=SystemRoles.Admin || status!="active")
            && await repository.CountActiveAdminsAsync(ct)<=1)
            throw new IdentityApiException(409,"last_admin","Hệ thống phải còn ít nhất một admin hoạt động.");
        var updated=await repository.UpdateUserAsync(id,Required(request.FullName,"fullName",150),role,status,ct)
            ?? throw new IdentityApiException(404,"user_not_found","Không tìm thấy người dùng.");
        await repository.WriteAuditAsync(currentUser.UserId,"user_updated","user",id.ToString(),new{updated.Role,updated.Status},ip,ct);
        return ToProfile(updated);
    }

    public async Task ResetPasswordAsync(long id,ResetPasswordRequest request,string? ip,CancellationToken ct)
    {
        var user=await RequireUserAsync(id,ct);
        ValidatePassword(request.TemporaryPassword);
        await repository.SetPasswordAsync(id,passwordHasher.HashPassword(user,request.TemporaryPassword!),true,ct);
        await repository.WriteAuditAsync(currentUser.UserId,"password_reset","user",id.ToString(),null,ip,ct);
    }

    public async Task ReplaceCourseAssignmentsAsync(long id,long[]? ids,string? ip,CancellationToken ct)
    {
        var normalized=(ids??[]).Where(x=>x>0).Distinct().ToArray();
        await repository.ReplaceCourseAssignmentsAsync(id,currentUser.UserId,normalized,ct);
        await repository.WriteAuditAsync(currentUser.UserId,"course_assignments_replaced","user",id.ToString(),new{ids=normalized},ip,ct);
    }

    public async Task ReplaceProgramAssignmentsAsync(long id,long[]? ids,string? ip,CancellationToken ct)
    {
        var normalized=(ids??[]).Where(x=>x>0).Distinct().ToArray();
        await repository.ReplaceProgramAssignmentsAsync(id,currentUser.UserId,normalized,ct);
        await repository.WriteAuditAsync(currentUser.UserId,"program_assignments_replaced","user",id.ToString(),new{ids=normalized},ip,ct);
    }

    private async Task<IssuedSession> IssueSessionAsync(SecurityUser user,string? ip,CancellationToken ct)
    {
        var access=tokens.CreateAccessToken(user);
        var refresh=tokens.CreateRefreshToken();
        await repository.SaveRefreshTokenAsync(refresh.Id,user.Id,refresh.Hash,refresh.ExpiresAt,ip,ct);
        return new IssuedSession(new LoginResponse(access.Token,access.ExpiresAt,ToProfile(user)),refresh.Raw,refresh.ExpiresAt);
    }

    private async Task<SecurityUser> RequireUserAsync(long id,CancellationToken ct) =>
        await repository.FindByIdAsync(id,ct) ?? throw new IdentityApiException(404,"user_not_found","Không tìm thấy người dùng.");

    private static UserProfile ToProfile(SecurityUser user) => new(user.Id,user.Email,user.FullName,user.Role,user.Status,user.MustChangePassword,user.LastLoginAt,user.CreatedAt);
    private static IdentityApiException InvalidCredentials() => new(401,"invalid_credentials","Email hoặc mật khẩu không đúng.");
    private static int NormalizePage(int value)=>value<1?1:value;
    private static int NormalizePageSize(int value)=>Math.Clamp(value,1,100);
    private static string? Clean(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
    private static string Required(string? value,string field,int max)
    {
        var result=Clean(value);
        if(result is null || result.Length>max) throw new IdentityApiException(400,"validation_failed",$"{field} không hợp lệ.");
        return result;
    }
    private static string NormalizeEmail(string? value)
    {
        var email=Required(value,"email",254).ToLowerInvariant();
        try { _=new MailAddress(email); }
        catch(FormatException) { throw new IdentityApiException(400,"validation_failed","Email không hợp lệ."); }
        return email;
    }
    private static string ValidateRole(string? role)
    {
        var value=Clean(role)?.ToLowerInvariant();
        return value is SystemRoles.Admin or SystemRoles.Lecturer or SystemRoles.Student
            ? value : throw new IdentityApiException(400,"validation_failed","Vai trò không hợp lệ.");
    }
    private static string ValidateStatus(string? status)
    {
        var value=Clean(status)?.ToLowerInvariant();
        return value is "active" or "disabled" ? value
            : throw new IdentityApiException(400,"validation_failed","Trạng thái không hợp lệ.");
    }
    public static void ValidatePassword(string? password)
    {
        if(password is null || password.Length<10 || !password.Any(char.IsUpper)
            || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            throw new IdentityApiException(400,"weak_password","Mật khẩu phải có ít nhất 10 ký tự, gồm chữ hoa, chữ thường và chữ số.");
    }
}
