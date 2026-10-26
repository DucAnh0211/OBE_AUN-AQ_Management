using System.Text.Json;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ObeAunQa.SharedKernel;
using ObeAunQa.Modules.Identity.Infrastructure;
using ObeAunQa.Modules.Identity.Domain.Entities;

namespace ObeAunQa.Modules.Identity;

public sealed class IdentityRepository(IdentityDbContext db) : IIdentityRepository, IAssignmentCloneService
{
    private static readonly Expression<Func<User, SecurityUser>> UserDto = u => new(u.Id,u.Email,u.NormalizedEmail,u.FullName,u.PasswordHash,u.Role,u.Status,u.MustChangePassword,u.TokenVersion,u.FailedLoginCount,u.LockedUntil,u.LastLoginAt,u.CreatedAt,u.UpdatedAt);
    public Task<SecurityUser?> FindByEmailAsync(string email,CancellationToken ct) => db.Users.AsNoTracking().Where(u=>u.NormalizedEmail==email).Select(UserDto).SingleOrDefaultAsync(ct);
    public Task<SecurityUser?> FindByIdAsync(long id,CancellationToken ct) => db.Users.AsNoTracking().Where(u=>u.Id==id).Select(UserDto).SingleOrDefaultAsync(ct);
    public Task<long> CountActiveAdminsAsync(CancellationToken ct) => db.Users.LongCountAsync(u=>u.Role=="admin" && u.Status=="active",ct);
    public async Task<SecurityUser> CreateUserAsync(string email,string normalizedEmail,string fullName,string passwordHash,string role,bool mustChangePassword,CancellationToken ct)
    {
        var user=new User { Email=email,NormalizedEmail=normalizedEmail,FullName=fullName,PasswordHash=passwordHash,Role=role,Status="active",MustChangePassword=mustChangePassword };
        db.Users.Add(user);
        try { await db.SaveChangesAsync(ct); }
        catch(DbUpdateException e) when(e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { throw new IdentityApiException(409,"email_exists","Email đã được sử dụng."); }
        return (await FindByIdAsync(user.Id,ct))!;
    }
    public async Task RecordFailedLoginAsync(long id,bool lockAccount,CancellationToken ct) =>
        await db.Users.Where(u=>u.Id==id).ExecuteUpdateAsync(s=>s.SetProperty(u=>u.FailedLoginCount,u=>lockAccount?0:u.FailedLoginCount+1)
            .SetProperty(u=>u.LockedUntil,u=>lockAccount?DateTime.UtcNow.AddMinutes(15):u.LockedUntil).SetProperty(u=>u.UpdatedAt,DateTime.UtcNow),ct);
    public async Task RecordSuccessfulLoginAsync(long id,CancellationToken ct) =>
        await db.Users.Where(u=>u.Id==id).ExecuteUpdateAsync(s=>s.SetProperty(u=>u.FailedLoginCount,0).SetProperty(u=>u.LockedUntil,(DateTime?)null)
            .SetProperty(u=>u.LastLoginAt,DateTime.UtcNow).SetProperty(u=>u.UpdatedAt,DateTime.UtcNow),ct);
    public async Task SaveRefreshTokenAsync(Guid id,long userId,string hash,DateTimeOffset expiresAt,string? ip,CancellationToken ct)
    {
        db.RefreshTokens.Add(new RefreshToken {Id=id,UserId=userId,TokenHash=hash,ExpiresAt=expiresAt.UtcDateTime,CreatedByIp=ip});
        await db.SaveChangesAsync(ct);
    }
    public Task<RefreshTokenRecord?> FindRefreshTokenAsync(string hash,CancellationToken ct) =>
        db.RefreshTokens.AsNoTracking().Where(t=>t.TokenHash==hash).Select(t=>new RefreshTokenRecord(t.Id,t.UserId,t.TokenHash,t.ExpiresAt,t.RevokedAt)).SingleOrDefaultAsync(ct);
    public async Task RotateRefreshTokenAsync(Guid oldId,Guid newId,string? ip,CancellationToken ct) =>
        await db.RefreshTokens.Where(t=>t.Id==oldId && t.RevokedAt==null).ExecuteUpdateAsync(s=>s.SetProperty(t=>t.RevokedAt,DateTime.UtcNow).SetProperty(t=>t.RevokedByIp,ip).SetProperty(t=>t.ReplacedByTokenId,newId),ct);
    public async Task RevokeRefreshTokenAsync(string hash,string? ip,CancellationToken ct) =>
        await db.RefreshTokens.Where(t=>t.TokenHash==hash).ExecuteUpdateAsync(s=>s.SetProperty(t=>t.RevokedAt,t=>t.RevokedAt??DateTime.UtcNow).SetProperty(t=>t.RevokedByIp,t=>t.RevokedByIp??ip),ct);
    private Task<int> RevokeTokens(long id,CancellationToken ct) => db.RefreshTokens.Where(t=>t.UserId==id && t.RevokedAt==null).ExecuteUpdateAsync(s=>s.SetProperty(t=>t.RevokedAt,DateTime.UtcNow),ct);
    public async Task RevokeAllSessionsAsync(long id,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await db.Users.Where(u=>u.Id==id).ExecuteUpdateAsync(s=>s.SetProperty(u=>u.TokenVersion,u=>u.TokenVersion+1).SetProperty(u=>u.UpdatedAt,DateTime.UtcNow),ct);
        await RevokeTokens(id,ct);await tx.CommitAsync(ct);
    }
    public async Task<PagedUsers> GetUsersAsync(int page,int pageSize,string? search,CancellationToken ct)
    {
        var q=db.Users.AsNoTracking().Where(u=>search==null || EF.Functions.ILike(u.Email,"%"+search+"%") || EF.Functions.ILike(u.FullName,"%"+search+"%"));
        var count=await q.LongCountAsync(ct);
        var rows=await q.OrderBy(u=>u.FullName).ThenBy(u=>u.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(u=>new UserProfile(u.Id,u.Email,u.FullName,u.Role,u.Status,u.MustChangePassword,u.LastLoginAt,u.CreatedAt)).ToListAsync(ct);
        return new(rows,page,pageSize,count,(int)Math.Ceiling(count/(double)pageSize));
    }
    public async Task<SecurityUser?> UpdateUserAsync(long id,string fullName,string role,string status,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        if(await db.Users.Where(u=>u.Id==id).ExecuteUpdateAsync(s=>s.SetProperty(u=>u.FullName,fullName).SetProperty(u=>u.Role,role).SetProperty(u=>u.Status,status)
            .SetProperty(u=>u.TokenVersion,u=>u.TokenVersion+1).SetProperty(u=>u.UpdatedAt,DateTime.UtcNow),ct)==0) return null;
        await RevokeTokens(id,ct);
        if(role!="lecturer") await db.LecturerCourseAssignments.Where(a=>a.UserId==id).ExecuteDeleteAsync(ct);
        if(role!="student") await db.StudentProgramAssignments.Where(a=>a.UserId==id).ExecuteDeleteAsync(ct);
        await tx.CommitAsync(ct);return await FindByIdAsync(id,ct);
    }
    public async Task SetPasswordAsync(long id,string passwordHash,bool mustChange,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await db.Users.Where(u=>u.Id==id).ExecuteUpdateAsync(s=>s.SetProperty(u=>u.PasswordHash,passwordHash).SetProperty(u=>u.MustChangePassword,mustChange)
            .SetProperty(u=>u.TokenVersion,u=>u.TokenVersion+1).SetProperty(u=>u.FailedLoginCount,0).SetProperty(u=>u.LockedUntil,(DateTime?)null).SetProperty(u=>u.UpdatedAt,DateTime.UtcNow),ct);
        await RevokeTokens(id,ct);await tx.CommitAsync(ct);
    }
    public Task<long[]> GetCourseAssignmentsAsync(long userId,CancellationToken ct) => db.LecturerCourseAssignments.Where(a=>a.UserId==userId).OrderBy(a=>a.ProgramCourseId).Select(a=>a.ProgramCourseId).ToArrayAsync(ct);
    public Task<long[]> GetProgramAssignmentsAsync(long userId,CancellationToken ct) => db.StudentProgramAssignments.Where(a=>a.UserId==userId).OrderBy(a=>a.ProgramId).Select(a=>a.ProgramId).ToArrayAsync(ct);
    public async Task ReplaceCourseAssignmentsAsync(long userId,long actorId,long[] ids,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await RequireRole(userId,"lecturer",ct);ids=ids.Distinct().ToArray();
        CheckTargets(await db.Set<CourseAccessRow>().CountAsync(c=>ids.Contains(c.Id),ct)==ids.Length);
        await db.LecturerCourseAssignments.Where(a=>a.UserId==userId).ExecuteDeleteAsync(ct);
        db.LecturerCourseAssignments.AddRange(ids.Select(id=>new LecturerCourseAssignment{UserId=userId,ProgramCourseId=id,AssignedBy=actorId}));
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public async Task ReplaceProgramAssignmentsAsync(long userId,long actorId,long[] ids,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await RequireRole(userId,"student",ct);ids=ids.Distinct().ToArray();
        CheckTargets(await db.Set<ProgramAccessRow>().CountAsync(p=>ids.Contains(p.Id),ct)==ids.Length);
        await db.StudentProgramAssignments.Where(a=>a.UserId==userId).ExecuteDeleteAsync(ct);
        db.StudentProgramAssignments.AddRange(ids.Select(id=>new StudentProgramAssignment{UserId=userId,ProgramId=id,AssignedBy=actorId}));
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    private async Task RequireRole(long id,string role,CancellationToken ct)
    {
        if(!await db.Users.AnyAsync(u=>u.Id==id && u.Role==role && u.Status=="active",ct))
            throw new IdentityApiException(409,"invalid_assignment_role","Vai trò người dùng không phù hợp với loại phân công.");
    }
    private static void CheckTargets(bool valid)
    { if(!valid) throw new IdentityApiException(400,"invalid_assignment","Danh sách phân công chứa dữ liệu không tồn tại."); }
    public async Task CopyLecturerAssignmentsAsync(long sourceVersionId,long targetVersionId,CancellationToken ct)
    {
        var rows=await (from a in db.LecturerCourseAssignments
            join source in db.Set<CourseAccessRow>() on a.ProgramCourseId equals source.Id
            join target in db.Set<CourseAccessRow>() on source.CourseId equals target.CourseId
            where source.ProgramVersionId==sourceVersionId && target.ProgramVersionId==targetVersionId
                && !db.LecturerCourseAssignments.Any(x=>x.UserId==a.UserId && x.ProgramCourseId==target.Id)
            select new {a.UserId,ProgramCourseId=target.Id,a.AssignedBy}).ToListAsync(ct);
        db.LecturerCourseAssignments.AddRange(rows.Select(a=>new LecturerCourseAssignment{UserId=a.UserId,ProgramCourseId=a.ProgramCourseId,AssignedBy=a.AssignedBy}));
        await db.SaveChangesAsync(ct);
    }
    public async Task WriteAuditAsync(long? actor,string action,string targetType,string? targetId,object? details,string? ip,CancellationToken ct)
    {
        db.AuditLogs.Add(new AuditLog{ActorUserId=actor,Action=action,TargetType=targetType,TargetId=targetId,Details=JsonSerializer.Serialize(details??new{}),IpAddress=ip});
        await db.SaveChangesAsync(ct);
    }
    public async Task<PagedAuditLogs> GetAuditLogsAsync(int page,int pageSize,CancellationToken ct)
    {
        var count=await db.AuditLogs.LongCountAsync(ct);
        var rows=await db.AuditLogs.AsNoTracking().OrderByDescending(a=>a.CreatedAt).ThenByDescending(a=>a.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(a=>new AuditLogResponse(a.Id,a.ActorUserId,a.Action,a.TargetType,a.TargetId,a.Details,a.IpAddress,a.CreatedAt)).ToListAsync(ct);
        return new(rows,page,pageSize,count,(int)Math.Ceiling(count/(double)pageSize));
    }
    public Task<bool> IsUserTokenValidAsync(long id,int tokenVersion,CancellationToken ct) => db.Users.AnyAsync(u=>u.Id==id && u.Status=="active" && u.TokenVersion==tokenVersion,ct);
}
