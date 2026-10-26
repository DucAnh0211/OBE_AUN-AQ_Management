using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Identity;
public interface IIdentityRepository : IAssignmentCloneService
{
    Task<SecurityUser?> FindByEmailAsync(string email,CancellationToken ct);
    Task<SecurityUser?> FindByIdAsync(long id,CancellationToken ct);
    Task<long> CountActiveAdminsAsync(CancellationToken ct);
    Task<SecurityUser> CreateUserAsync(string email,string normalizedEmail,string fullName,string passwordHash,string role,bool mustChangePassword,CancellationToken ct);
    Task RecordFailedLoginAsync(long id,bool lockAccount,CancellationToken ct);
    Task RecordSuccessfulLoginAsync(long id,CancellationToken ct);
    Task SaveRefreshTokenAsync(Guid id,long userId,string hash,DateTimeOffset expiresAt,string? ip,CancellationToken ct);
    Task<RefreshTokenRecord?> FindRefreshTokenAsync(string hash,CancellationToken ct);
    Task RotateRefreshTokenAsync(Guid oldId,Guid newId,string? ip,CancellationToken ct);
    Task RevokeRefreshTokenAsync(string hash,string? ip,CancellationToken ct);
    Task RevokeAllSessionsAsync(long id,CancellationToken ct);
    Task<PagedUsers> GetUsersAsync(int page,int pageSize,string? search,CancellationToken ct);
    Task<SecurityUser?> UpdateUserAsync(long id,string fullName,string role,string status,CancellationToken ct);
    Task SetPasswordAsync(long id,string passwordHash,bool mustChange,CancellationToken ct);
    Task<long[]> GetCourseAssignmentsAsync(long userId,CancellationToken ct);
    Task<long[]> GetProgramAssignmentsAsync(long userId,CancellationToken ct);
    Task ReplaceCourseAssignmentsAsync(long userId,long actorId,long[] ids,CancellationToken ct);
    Task ReplaceProgramAssignmentsAsync(long userId,long actorId,long[] ids,CancellationToken ct);
    Task WriteAuditAsync(long? actor,string action,string targetType,string? targetId,object? details,string? ip,CancellationToken ct);
    Task<PagedAuditLogs> GetAuditLogsAsync(int page,int pageSize,CancellationToken ct);
    Task<bool> IsUserTokenValidAsync(long id,int tokenVersion,CancellationToken ct);
}
