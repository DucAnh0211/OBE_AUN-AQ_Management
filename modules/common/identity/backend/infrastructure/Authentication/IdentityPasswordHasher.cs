using Microsoft.AspNetCore.Identity;
namespace ObeAunQa.Modules.Identity;
internal sealed class IdentityPasswordHasher(IPasswordHasher<SecurityUser> hasher) : IIdentityPasswordHasher
{
    public string HashPassword(SecurityUser user,string password) => hasher.HashPassword(user,password);
    public PasswordCheckResult VerifyHashedPassword(SecurityUser user,string hash,string password) =>
        hasher.VerifyHashedPassword(user,hash,password) switch {
            PasswordVerificationResult.Success=>PasswordCheckResult.Success,
            PasswordVerificationResult.SuccessRehashNeeded=>PasswordCheckResult.SuccessRehashNeeded,
            _=>PasswordCheckResult.Failed
        };
}
