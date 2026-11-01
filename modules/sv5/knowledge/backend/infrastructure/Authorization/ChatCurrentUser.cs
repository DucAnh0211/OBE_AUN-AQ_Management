using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

// Constructed only from the persisted thread owner, never from model arguments.
internal sealed record ChatCurrentUser(long UserId, string Role) : ICurrentUser
{
    public bool IsAuthenticated => true;
    public bool MustChangePassword => false;
}
