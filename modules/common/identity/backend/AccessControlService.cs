using Dapper;
using Npgsql;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Identity;

public sealed class AccessControlService(NpgsqlDataSource dataSource, ICurrentUser currentUser)
    : IAccessControlService
{
    public async Task<bool> CanReadProgramAsync(long programId, CancellationToken ct)
    {
        if (currentUser.Role == SystemRoles.Admin) return true;
        const string sql = """
            SELECT CASE
              WHEN @Role='lecturer' THEN EXISTS(
                SELECT 1 FROM security.lecturer_course_assignments a
                JOIN curriculum.program_courses pc ON pc.id=a.program_course_id
                JOIN curriculum.program_versions pv ON pv.id=pc.program_version_id
                WHERE a.user_id=@UserId AND pv.program_id=@ProgramId)
              WHEN @Role='student' THEN EXISTS(
                SELECT 1 FROM security.student_program_assignments a
                JOIN curriculum.program_versions pv ON pv.program_id=a.program_id
                WHERE a.user_id=@UserId AND a.program_id=@ProgramId
                  AND pv.dataset_status='finalized' AND pv.is_current AND pv.archived_at IS NULL)
              ELSE false END;
            """;
        return await ScalarAsync(sql,new{currentUser.Role,currentUser.UserId,ProgramId=programId},ct);
    }

    public async Task<bool> CanReadVersionAsync(long versionId, CancellationToken ct)
    {
        if (currentUser.Role == SystemRoles.Admin) return true;
        const string sql = """
            SELECT CASE
              WHEN @Role='lecturer' THEN EXISTS(
                SELECT 1 FROM security.lecturer_course_assignments a
                JOIN curriculum.program_courses pc ON pc.id=a.program_course_id
                WHERE a.user_id=@UserId AND pc.program_version_id=@VersionId)
              WHEN @Role='student' THEN EXISTS(
                SELECT 1 FROM curriculum.program_versions pv
                JOIN security.student_program_assignments a ON a.program_id=pv.program_id
                WHERE a.user_id=@UserId AND pv.id=@VersionId
                  AND pv.dataset_status='finalized' AND pv.is_current AND pv.archived_at IS NULL)
              ELSE false END;
            """;
        return await ScalarAsync(sql,new{currentUser.Role,currentUser.UserId,VersionId=versionId},ct);
    }

    public async Task<bool> CanReadCourseAsync(long programCourseId, CancellationToken ct)
    {
        if (currentUser.Role == SystemRoles.Admin) return true;
        const string sql = """
            SELECT CASE
              WHEN @Role='lecturer' THEN EXISTS(
                SELECT 1 FROM security.lecturer_course_assignments
                WHERE user_id=@UserId AND program_course_id=@CourseId)
              WHEN @Role='student' THEN EXISTS(
                SELECT 1 FROM curriculum.program_courses pc
                JOIN curriculum.program_versions pv ON pv.id=pc.program_version_id
                JOIN security.student_program_assignments a ON a.program_id=pv.program_id
                WHERE a.user_id=@UserId AND pc.id=@CourseId AND pc.archived_at IS NULL
                  AND pv.dataset_status='finalized' AND pv.is_current AND pv.archived_at IS NULL)
              ELSE false END;
            """;
        return await ScalarAsync(sql,new{currentUser.Role,currentUser.UserId,CourseId=programCourseId},ct);
    }

    public async Task<bool> CanEditCourseOutcomesAsync(long programCourseId, CancellationToken ct)
    {
        if (currentUser.Role == SystemRoles.Admin) return true;
        if (currentUser.Role != SystemRoles.Lecturer) return false;
        const string sql = """
            SELECT EXISTS(
              SELECT 1 FROM security.lecturer_course_assignments a
              JOIN curriculum.program_courses pc ON pc.id=a.program_course_id
              JOIN curriculum.program_versions pv ON pv.id=pc.program_version_id
              WHERE a.user_id=@UserId AND pc.id=@CourseId
                AND pc.archived_at IS NULL AND pv.dataset_status='draft' AND pv.archived_at IS NULL);
            """;
        return await ScalarAsync(sql,new{currentUser.UserId,CourseId=programCourseId},ct);
    }

    public async Task<long[]> GetReadableProgramIdsAsync(CancellationToken ct)
    {
        if (currentUser.Role == SystemRoles.Admin) return [];
        var sql = currentUser.Role == SystemRoles.Lecturer
            ? """
              SELECT DISTINCT pv.program_id FROM security.lecturer_course_assignments a
              JOIN curriculum.program_courses pc ON pc.id=a.program_course_id
              JOIN curriculum.program_versions pv ON pv.id=pc.program_version_id
              WHERE a.user_id=@UserId;
              """
            : """
              SELECT DISTINCT a.program_id FROM security.student_program_assignments a
              JOIN curriculum.program_versions pv ON pv.program_id=a.program_id
              WHERE a.user_id=@UserId AND pv.dataset_status='finalized'
                AND pv.is_current AND pv.archived_at IS NULL;
              """;
        return await QueryIdsAsync(sql,new{currentUser.UserId},ct);
    }

    public async Task<long[]> GetReadableCourseIdsAsync(long versionId, CancellationToken ct)
    {
        if (currentUser.Role == SystemRoles.Admin) return [];
        if (currentUser.Role == SystemRoles.Lecturer)
            return await QueryIdsAsync("""
                SELECT pc.id FROM security.lecturer_course_assignments a
                JOIN curriculum.program_courses pc ON pc.id=a.program_course_id
                WHERE a.user_id=@UserId AND pc.program_version_id=@VersionId;
                """,new{currentUser.UserId,VersionId=versionId},ct);
        return await QueryIdsAsync("""
            SELECT pc.id FROM curriculum.program_courses pc
            JOIN curriculum.program_versions pv ON pv.id=pc.program_version_id
            JOIN security.student_program_assignments a ON a.program_id=pv.program_id
            WHERE a.user_id=@UserId AND pv.id=@VersionId AND pv.dataset_status='finalized'
              AND pv.is_current AND pv.archived_at IS NULL AND pc.archived_at IS NULL;
            """,new{currentUser.UserId,VersionId=versionId},ct);
    }

    public async Task<bool> CanReadFrameworkAsync(long frameworkId, CancellationToken ct)
    {
        if (currentUser.Role == SystemRoles.Admin) return true;
        if (currentUser.Role != SystemRoles.Lecturer) return false;
        return await ScalarAsync(
            "SELECT EXISTS(SELECT 1 FROM accreditation.aun_frameworks WHERE id=@Id AND status='published');",
            new{Id=frameworkId},ct);
    }

    private async Task<bool> ScalarAsync(string sql,object args,CancellationToken ct)
    {
        await using var connection=await dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql,args,cancellationToken:ct));
    }

    private async Task<long[]> QueryIdsAsync(string sql,object args,CancellationToken ct)
    {
        await using var connection=await dataSource.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<long>(new CommandDefinition(sql,args,cancellationToken:ct))).ToArray();
    }
}
