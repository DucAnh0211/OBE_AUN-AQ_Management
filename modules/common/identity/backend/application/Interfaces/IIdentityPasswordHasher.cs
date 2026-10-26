namespace ObeAunQa.Modules.Identity;
public interface IIdentityPasswordHasher
{
    string HashPassword(SecurityUser user, string password);
    PasswordCheckResult VerifyHashedPassword(SecurityUser user, string hash, string password);
}
