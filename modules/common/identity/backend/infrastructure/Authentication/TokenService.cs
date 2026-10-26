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

public sealed class TokenService(IOptions<JwtOptions> options) : IIdentityTokenService
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
    string IIdentityTokenService.Hash(string raw) => Hash(raw);
}
