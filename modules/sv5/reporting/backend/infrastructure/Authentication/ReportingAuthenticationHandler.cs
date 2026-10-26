using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

// shortcut: configured access keys until shared IAM is implemented.
public sealed class ReportingAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    IOptionsMonitor<ReportingAccessOptions> access)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ReportingAccess";
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());
        var token = header[7..];
        var user = FindUser(access.CurrentValue, token);
        if (user is null) return Task.FromResult(AuthenticateResult.Fail("Mã truy cập không hợp lệ hoặc đã bị thu hồi."));
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id), new(ClaimTypes.Name, user.Id) };
        claims.AddRange(user.ProgramIds.Distinct().Select(id => new Claim("reporting:program", id.ToString(System.Globalization.CultureInfo.InvariantCulture))));
        if (user.CanGenerate) claims.Add(new("reporting:generate", "true"));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }

    public static ReportingUser? FindUser(ReportingAccessOptions access, string token)
    {
        if (token.Length is < 32 or > 256) return null;
        var actual = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        foreach (var user in access.Users)
        {
            if (string.IsNullOrWhiteSpace(user.Id) || user.TokenSha256.Length != 64) continue;
            byte[] expected;
            try { expected = Convert.FromHexString(user.TokenSha256); }
            catch (FormatException) { continue; }
            if (CryptographicOperations.FixedTimeEquals(actual, expected)) return user;
        }
        return null;
    }
}

