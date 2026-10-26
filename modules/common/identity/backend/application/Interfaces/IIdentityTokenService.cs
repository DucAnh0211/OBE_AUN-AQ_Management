namespace ObeAunQa.Modules.Identity;
public interface IIdentityTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(SecurityUser user);
    (Guid Id, string Raw, string Hash, DateTimeOffset ExpiresAt) CreateRefreshToken();
    string Hash(string raw);
}
