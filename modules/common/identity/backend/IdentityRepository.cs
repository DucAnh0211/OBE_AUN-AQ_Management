using System.Text.Json;
using Dapper;
using Npgsql;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Identity;

public sealed class IdentityRepository(NpgsqlDataSource dataSource) : IAssignmentCloneService
{
    private const string UserProjection = """
        SELECT id AS Id, email AS Email, normalized_email AS NormalizedEmail,
               full_name AS FullName, password_hash AS PasswordHash, role AS Role,
               status AS Status, must_change_password AS MustChangePassword,
               token_version AS TokenVersion, failed_login_count AS FailedLoginCount,
               locked_until AS LockedUntil, last_login_at AS LastLoginAt,
               created_at AS CreatedAt, updated_at AS UpdatedAt
        FROM security.users
        """;

    public async Task<SecurityUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<SecurityUser>(new CommandDefinition(
            UserProjection + " WHERE normalized_email=@NormalizedEmail;",
            new { NormalizedEmail = normalizedEmail }, cancellationToken: ct));
    }

    public async Task<SecurityUser?> FindByIdAsync(long id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<SecurityUser>(new CommandDefinition(
            UserProjection + " WHERE id=@Id;", new { Id = id }, cancellationToken: ct));
    }

    public async Task<long> CountActiveAdminsAsync(CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) FROM security.users WHERE role='admin' AND status='active';",
            cancellationToken: ct));
    }

    public async Task<SecurityUser> CreateUserAsync(string email, string normalizedEmail,
        string fullName, string passwordHash, string role, bool mustChangePassword,
        CancellationToken ct)
    {
        const string sql = """
            INSERT INTO security.users(email,normalized_email,full_name,password_hash,role,must_change_password)
            VALUES(@Email,@NormalizedEmail,@FullName,@PasswordHash,@Role,@MustChangePassword)
            RETURNING id;
            """;
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(ct);
            var id = await connection.ExecuteScalarAsync<long>(new CommandDefinition(sql,
                new { Email=email, NormalizedEmail=normalizedEmail, FullName=fullName,
                    PasswordHash=passwordHash, Role=role, MustChangePassword=mustChangePassword },
                cancellationToken:ct));
            return (await FindByIdAsync(id, ct))!;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new IdentityApiException(409, "email_exists", "Email đã được sử dụng.");
        }
    }

    public Task RecordFailedLoginAsync(long userId, bool lockAccount, CancellationToken ct) =>
        ExecuteAsync("""
            UPDATE security.users SET
                failed_login_count = CASE WHEN @LockAccount THEN 0 ELSE failed_login_count + 1 END,
                locked_until = CASE WHEN @LockAccount THEN now() + interval '15 minutes' ELSE locked_until END,
                updated_at=now()
            WHERE id=@UserId;
            """, new { UserId=userId, LockAccount=lockAccount }, ct);

    public Task RecordSuccessfulLoginAsync(long userId, CancellationToken ct) =>
        ExecuteAsync("UPDATE security.users SET failed_login_count=0,locked_until=NULL,last_login_at=now(),updated_at=now() WHERE id=@UserId;",
            new { UserId=userId }, ct);

    public Task SaveRefreshTokenAsync(Guid id, long userId, string hash,
        DateTimeOffset expiresAt, string? ip, CancellationToken ct) =>
        ExecuteAsync("""
            INSERT INTO security.refresh_tokens(id,user_id,token_hash,expires_at,created_by_ip)
            VALUES(@Id,@UserId,@Hash,@ExpiresAt,@Ip);
            """, new { Id=id, UserId=userId, Hash=hash, ExpiresAt=expiresAt, Ip=ip }, ct);

    public async Task<RefreshTokenRecord?> FindRefreshTokenAsync(string hash, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<RefreshTokenRecord>(new CommandDefinition("""
            SELECT id AS Id,user_id AS UserId,token_hash AS TokenHash,
                   expires_at AS ExpiresAt,revoked_at AS RevokedAt
            FROM security.refresh_tokens WHERE token_hash=@Hash;
            """, new { Hash=hash }, cancellationToken:ct));
    }

    public Task RotateRefreshTokenAsync(Guid oldId, Guid newId, string? ip, CancellationToken ct) =>
        ExecuteAsync("UPDATE security.refresh_tokens SET revoked_at=now(),revoked_by_ip=@Ip,replaced_by_token_id=@NewId WHERE id=@OldId AND revoked_at IS NULL;",
            new { OldId=oldId, NewId=newId, Ip=ip }, ct);

    public Task RevokeRefreshTokenAsync(string hash, string? ip, CancellationToken ct) =>
        ExecuteAsync("UPDATE security.refresh_tokens SET revoked_at=COALESCE(revoked_at,now()),revoked_by_ip=COALESCE(revoked_by_ip,@Ip) WHERE token_hash=@Hash;",
            new { Hash=hash, Ip=ip }, ct);

    public Task RevokeAllSessionsAsync(long userId, CancellationToken ct) =>
        ExecuteAsync("""
            UPDATE security.users SET token_version=token_version+1,updated_at=now() WHERE id=@UserId;
            UPDATE security.refresh_tokens SET revoked_at=COALESCE(revoked_at,now()) WHERE user_id=@UserId;
            """, new { UserId=userId }, ct);

    public async Task<PagedUsers> GetUsersAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        const string sql = """
            SELECT COUNT(*) FROM security.users
            WHERE @Search IS NULL OR email ILIKE '%'||@Search||'%' OR full_name ILIKE '%'||@Search||'%';
            SELECT id AS Id,email AS Email,full_name AS FullName,role AS Role,status AS Status,
                   must_change_password AS MustChangePassword,last_login_at AS LastLoginAt,created_at AS CreatedAt
            FROM security.users
            WHERE @Search IS NULL OR email ILIKE '%'||@Search||'%' OR full_name ILIKE '%'||@Search||'%'
            ORDER BY full_name,id LIMIT @PageSize OFFSET @Offset;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        using var result = await connection.QueryMultipleAsync(new CommandDefinition(sql,
            new { Search=search, PageSize=pageSize, Offset=(page-1)*pageSize }, cancellationToken:ct));
        var total = await result.ReadSingleAsync<long>();
        var items = (await result.ReadAsync<UserProfile>()).AsList();
        return new PagedUsers(items,page,pageSize,total,(int)Math.Ceiling(total/(double)pageSize));
    }

    public async Task<SecurityUser?> UpdateUserAsync(long id, string fullName,
        string role, string status, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        var exists = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "SELECT EXISTS(SELECT 1 FROM security.users WHERE id=@Id);",new{Id=id},transaction,cancellationToken:ct));
        if (!exists) return null;
        await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE security.users SET full_name=@FullName,role=@Role,status=@Status,
                token_version=token_version+1,updated_at=now() WHERE id=@Id;
            UPDATE security.refresh_tokens SET revoked_at=COALESCE(revoked_at,now()) WHERE user_id=@Id;
            """, new { Id=id, FullName=fullName, Role=role, Status=status }, transaction, cancellationToken:ct));
        if (role != "lecturer")
            await connection.ExecuteAsync(new CommandDefinition("DELETE FROM security.lecturer_course_assignments WHERE user_id=@Id;",new{Id=id},transaction,cancellationToken:ct));
        if (role != "student")
            await connection.ExecuteAsync(new CommandDefinition("DELETE FROM security.student_program_assignments WHERE user_id=@Id;",new{Id=id},transaction,cancellationToken:ct));
        await transaction.CommitAsync(ct);
        return await FindByIdAsync(id,ct);
    }

    public Task SetPasswordAsync(long id, string passwordHash, bool mustChange, CancellationToken ct) =>
        ExecuteAsync("""
            UPDATE security.users SET password_hash=@PasswordHash,must_change_password=@MustChange,
                token_version=token_version+1,failed_login_count=0,locked_until=NULL,updated_at=now() WHERE id=@Id;
            UPDATE security.refresh_tokens SET revoked_at=COALESCE(revoked_at,now()) WHERE user_id=@Id;
            """, new { Id=id, PasswordHash=passwordHash, MustChange=mustChange }, ct);

    public async Task<long[]> GetCourseAssignmentsAsync(long userId, CancellationToken ct) =>
        (await QueryAsync<long>("SELECT program_course_id FROM security.lecturer_course_assignments WHERE user_id=@UserId ORDER BY program_course_id;",new{UserId=userId},ct)).ToArray();

    public async Task<long[]> GetProgramAssignmentsAsync(long userId, CancellationToken ct) =>
        (await QueryAsync<long>("SELECT program_id FROM security.student_program_assignments WHERE user_id=@UserId ORDER BY program_id;",new{UserId=userId},ct)).ToArray();

    public Task ReplaceCourseAssignmentsAsync(long userId, long actorId, long[] ids, CancellationToken ct) =>
        ReplaceAssignmentsAsync(userId,actorId,ids,"lecturer","lecturer_course_assignments","program_course_id","curriculum.program_courses",ct);

    public Task ReplaceProgramAssignmentsAsync(long userId, long actorId, long[] ids, CancellationToken ct) =>
        ReplaceAssignmentsAsync(userId,actorId,ids,"student","student_program_assignments","program_id","curriculum.programs",ct);

    private async Task ReplaceAssignmentsAsync(long userId,long actorId,long[] ids,
        string requiredRole,string table,string column,string targetTable,CancellationToken ct)
    {
        await using var connection=await dataSource.OpenConnectionAsync(ct);
        await using var transaction=await connection.BeginTransactionAsync(ct);
        var role=await connection.QuerySingleOrDefaultAsync<string>(new CommandDefinition(
            "SELECT role FROM security.users WHERE id=@UserId AND status='active';",
            new{UserId=userId},transaction,cancellationToken:ct));
        if(role!=requiredRole)
            throw new IdentityApiException(409,"invalid_assignment_role","Vai trò người dùng không phù hợp với loại phân công.");
        if(ids.Length>0)
        {
            var count=await connection.ExecuteScalarAsync<long>(new CommandDefinition(
                $"SELECT COUNT(*) FROM {targetTable} WHERE id=ANY(@Ids);",new{Ids=ids.Distinct().ToArray()},transaction,cancellationToken:ct));
            if(count!=ids.Distinct().LongCount())
                throw new IdentityApiException(400,"invalid_assignment","Danh sách phân công chứa dữ liệu không tồn tại.");
        }
        await connection.ExecuteAsync(new CommandDefinition(
            $"DELETE FROM security.{table} WHERE user_id=@UserId;",new{UserId=userId},transaction,cancellationToken:ct));
        foreach(var id in ids.Distinct())
            await connection.ExecuteAsync(new CommandDefinition(
                $"INSERT INTO security.{table}(user_id,{column},assigned_by) VALUES(@UserId,@TargetId,@ActorId);",
                new{UserId=userId,TargetId=id,ActorId=actorId},transaction,cancellationToken:ct));
        await transaction.CommitAsync(ct);
    }

    public Task CopyLecturerAssignmentsAsync(long sourceVersionId,long targetVersionId,CancellationToken ct) =>
        ExecuteAsync("""
            INSERT INTO security.lecturer_course_assignments(user_id,program_course_id,assigned_by)
            SELECT a.user_id,target.id,a.assigned_by
            FROM security.lecturer_course_assignments a
            JOIN curriculum.program_courses source ON source.id=a.program_course_id AND source.program_version_id=@SourceVersionId
            JOIN curriculum.program_courses target ON target.program_version_id=@TargetVersionId AND target.course_id=source.course_id
            ON CONFLICT DO NOTHING;
            """,new{SourceVersionId=sourceVersionId,TargetVersionId=targetVersionId},ct);

    public Task WriteAuditAsync(long? actor,string action,string targetType,string? targetId,
        object? details,string? ip,CancellationToken ct) =>
        ExecuteAsync("INSERT INTO security.audit_logs(actor_user_id,action,target_type,target_id,details,ip_address) VALUES(@Actor,@Action,@TargetType,@TargetId,CAST(@Details AS jsonb),@Ip);",
            new{Actor=actor,Action=action,TargetType=targetType,TargetId=targetId,
                Details=JsonSerializer.Serialize(details??new{}),Ip=ip},ct);

    public async Task<PagedAuditLogs> GetAuditLogsAsync(int page,int pageSize,CancellationToken ct)
    {
        const string sql="""
            SELECT COUNT(*) FROM security.audit_logs;
            SELECT id AS Id,actor_user_id AS ActorUserId,action AS Action,target_type AS TargetType,
                   target_id AS TargetId,details::text AS Details,ip_address AS IpAddress,created_at AS CreatedAt
            FROM security.audit_logs ORDER BY created_at DESC,id DESC LIMIT @PageSize OFFSET @Offset;
            """;
        await using var connection=await dataSource.OpenConnectionAsync(ct);
        using var result=await connection.QueryMultipleAsync(new CommandDefinition(sql,
            new{PageSize=pageSize,Offset=(page-1)*pageSize},cancellationToken:ct));
        var total=await result.ReadSingleAsync<long>();
        var items=(await result.ReadAsync<AuditLogResponse>()).AsList();
        return new PagedAuditLogs(items,page,pageSize,total,(int)Math.Ceiling(total/(double)pageSize));
    }

    public async Task<bool> IsUserTokenValidAsync(long id,int tokenVersion,CancellationToken ct)
    {
        await using var connection=await dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "SELECT EXISTS(SELECT 1 FROM security.users WHERE id=@Id AND status='active' AND token_version=@TokenVersion);",
            new{Id=id,TokenVersion=tokenVersion},cancellationToken:ct));
    }

    private async Task ExecuteAsync(string sql,object args,CancellationToken ct)
    {
        await using var connection=await dataSource.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(new CommandDefinition(sql,args,cancellationToken:ct));
    }

    private async Task<IEnumerable<T>> QueryAsync<T>(string sql,object args,CancellationToken ct)
    {
        await using var connection=await dataSource.OpenConnectionAsync(ct);
        return await connection.QueryAsync<T>(new CommandDefinition(sql,args,cancellationToken:ct));
    }
}
