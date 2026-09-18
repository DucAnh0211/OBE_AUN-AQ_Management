using Dapper;
using Npgsql;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

public sealed class NpgsqlCurriculumRepository(NpgsqlDataSource dataSource)
    : ICurriculumRepository
{
    private const string ProgramProjection = """
        SELECT p.id AS Id,
               p.code AS Code,
               p.name AS Name,
               (p.archived_at IS NOT NULL) AS IsArchived,
               COUNT(pv.id) FILTER (WHERE pv.archived_at IS NULL) AS VersionCount,
               current_version.id AS CurrentVersionId,
               current_version.version_code AS CurrentVersionCode,
               p.created_at AS CreatedAt,
               p.updated_at AS UpdatedAt
        FROM curriculum.programs p
        LEFT JOIN curriculum.program_versions pv ON pv.program_id = p.id
        LEFT JOIN curriculum.program_versions current_version
          ON current_version.program_id = p.id
         AND current_version.is_current
         AND current_version.archived_at IS NULL
        """;

    private const string VersionProjection = """
        SELECT pv.id AS Id,
               pv.program_id AS ProgramId,
               pv.version_code AS VersionCode,
               pv.dataset_status AS Status,
               pv.is_current AS IsCurrent,
               pv.source_version_id AS SourceVersionId,
               (
                   SELECT COUNT(*)
                   FROM curriculum.program_courses pc
                   WHERE pc.program_version_id = pv.id
                     AND pc.archived_at IS NULL
               ) AS CourseCount,
               pv.published_at AS PublishedAt,
               pv.archived_at AS ArchivedAt,
               pv.created_at AS CreatedAt,
               pv.updated_at AS UpdatedAt
        FROM curriculum.program_versions pv
        """;

    private const string CourseProjection = """
        SELECT pc.id AS Id,
               pc.program_version_id AS ProgramVersionId,
               pc.course_id AS CourseId,
               pc.course_code_snapshot AS InstitutionalCode,
               pc.course_name_snapshot AS Name,
               pc.credits AS Credits,
               pc.semester_source_value AS Semester,
               pc.display_order AS DisplayOrder,
               pc.dataset_status AS Status,
               pc.source_row AS SourceRow,
               pc.archived_at AS ArchivedAt,
               pc.created_at AS CreatedAt,
               pc.updated_at AS UpdatedAt
        FROM curriculum.program_courses pc
        """;

    public async Task<PagedResult<ProgramResponse>> GetProgramsAsync(
        int page,
        int pageSize,
        string? search,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM curriculum.programs p
            WHERE (@IncludeArchived OR p.archived_at IS NULL)
              AND (@Search IS NULL OR p.code ILIKE '%' || @Search || '%'
                   OR p.name ILIKE '%' || @Search || '%');

            """ + ProgramProjection + """

            WHERE (@IncludeArchived OR p.archived_at IS NULL)
              AND (@Search IS NULL OR p.code ILIKE '%' || @Search || '%'
                   OR p.name ILIKE '%' || @Search || '%')
            GROUP BY p.id, current_version.id, current_version.version_code
            ORDER BY p.code, p.id
            LIMIT @PageSize OFFSET @Offset;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new
            {
                IncludeArchived = includeArchived,
                Search = search,
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            },
            cancellationToken: cancellationToken);
        using var result = await connection.QueryMultipleAsync(command);
        var total = await result.ReadSingleAsync<long>();
        var items = (await result.ReadAsync<ProgramResponse>()).AsList();
        return PagedResult<ProgramResponse>.Create(items, page, pageSize, total);
    }

    public async Task<ProgramResponse?> GetProgramAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var sql = ProgramProjection + """

            WHERE p.id = @Id
            GROUP BY p.id, current_version.id, current_version.version_code;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ProgramResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<ProgramResponse> CreateProgramAsync(
        string code,
        string name,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO curriculum.programs (code, name)
            VALUES (@Code, @Name)
            RETURNING id;
            """;

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            var id = await connection.QuerySingleAsync<long>(
                new CommandDefinition(sql, new { Code = code, Name = name }, cancellationToken: cancellationToken));
            return (await GetProgramAsync(id, cancellationToken))!;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate("program_code_exists", "Mã chương trình đào tạo đã tồn tại.", exception);
        }
    }

    public async Task<ProgramResponse?> UpdateProgramAsync(
        long id,
        string name,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE curriculum.programs
            SET name = @Name, updated_at = now()
            WHERE id = @Id AND archived_at IS NULL
            RETURNING id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var updatedId = await connection.QuerySingleOrDefaultAsync<long?>(
            new CommandDefinition(sql, new { Id = id, Name = name }, cancellationToken: cancellationToken));
        return updatedId is null ? null : await GetProgramAsync(updatedId.Value, cancellationToken);
    }

    public async Task<bool> ArchiveProgramAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE curriculum.programs
            SET archived_at = now(), updated_at = now()
            WHERE id = @Id AND archived_at IS NULL;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken)) == 1;
    }

    public async Task<IReadOnlyList<ProgramVersionResponse>> GetProgramVersionsAsync(
        long programId,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        var sql = VersionProjection + """

            WHERE pv.program_id = @ProgramId
              AND (@IncludeArchived OR pv.archived_at IS NULL)
            ORDER BY pv.created_at DESC, pv.id DESC;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var versions = await connection.QueryAsync<ProgramVersionResponse>(
            new CommandDefinition(
                sql,
                new { ProgramId = programId, IncludeArchived = includeArchived },
                cancellationToken: cancellationToken));
        return versions.AsList();
    }

    public async Task<ProgramVersionResponse?> GetProgramVersionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await GetProgramVersionAsync(connection, null, id, cancellationToken);
    }

    public async Task<ProgramVersionResponse> CreateProgramVersionAsync(
        long programId,
        string versionCode,
        long? sourceVersionId,
        CancellationToken cancellationToken)
    {
        const string programExistsSql = """
            SELECT EXISTS (
                SELECT 1 FROM curriculum.programs
                WHERE id = @ProgramId AND archived_at IS NULL
            );
            """;
        const string sourceStatusSql = """
            SELECT dataset_status
            FROM curriculum.program_versions
            WHERE id = @SourceVersionId
              AND program_id = @ProgramId
              AND archived_at IS NULL;
            """;
        const string insertVersionSql = """
            INSERT INTO curriculum.program_versions
                (program_id, version_code, dataset_status, source_version_id)
            VALUES (@ProgramId, @VersionCode, 'draft', @SourceVersionId)
            RETURNING id;
            """;
        const string cloneCoursesSql = """
            INSERT INTO curriculum.program_courses
                (program_version_id, course_id, source_row, course_code_source_value,
                 credits, semester_source_value, dataset_status, quality_flags,
                 course_code_snapshot, course_name_snapshot, display_order)
            SELECT @NewVersionId, pc.course_id, pc.source_row, pc.course_code_source_value,
                   pc.credits, pc.semester_source_value, 'draft', pc.quality_flags,
                   pc.course_code_snapshot, pc.course_name_snapshot, pc.display_order
            FROM curriculum.program_courses pc
            WHERE pc.program_version_id = @SourceVersionId
              AND pc.archived_at IS NULL;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var programExists = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    programExistsSql,
                    new { ProgramId = programId },
                    transaction,
                    cancellationToken: cancellationToken));
            if (!programExists)
            {
                throw new CurriculumNotFoundException("chương trình đào tạo", programId);
            }

            if (sourceVersionId is not null)
            {
                var sourceStatus = await connection.QuerySingleOrDefaultAsync<string>(
                    new CommandDefinition(
                        sourceStatusSql,
                        new { ProgramId = programId, SourceVersionId = sourceVersionId },
                        transaction,
                        cancellationToken: cancellationToken));
                if (sourceStatus is null)
                {
                    throw new CurriculumNotFoundException(
                        "phiên bản chương trình đào tạo nguồn",
                        sourceVersionId.Value);
                }

                if (sourceStatus != CurriculumStatuses.Finalized)
                {
                    throw new CurriculumConflictException(
                        "source_version_not_finalized",
                        "Chỉ có thể sao chép học phần từ phiên bản đã finalized.");
                }
            }

            var versionId = await connection.QuerySingleAsync<long>(
                new CommandDefinition(
                    insertVersionSql,
                    new
                    {
                        ProgramId = programId,
                        VersionCode = versionCode,
                        SourceVersionId = sourceVersionId
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            if (sourceVersionId is not null)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        cloneCoursesSql,
                        new { NewVersionId = versionId, SourceVersionId = sourceVersionId },
                        transaction,
                        cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
            return (await GetProgramVersionAsync(versionId, cancellationToken))!;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate(
                "program_version_code_exists",
                "Mã phiên bản đã tồn tại trong chương trình đào tạo.",
                exception);
        }
    }

    public async Task<ProgramVersionResponse?> UpdateProgramVersionAsync(
        long id,
        string versionCode,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE curriculum.program_versions
            SET version_code = @VersionCode, updated_at = now()
            WHERE id = @Id
              AND dataset_status = 'draft'
              AND archived_at IS NULL
            RETURNING id;
            """;

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            var updatedId = await connection.QuerySingleOrDefaultAsync<long?>(
                new CommandDefinition(
                    sql,
                    new { Id = id, VersionCode = versionCode },
                    cancellationToken: cancellationToken));
            return updatedId is null
                ? null
                : await GetProgramVersionAsync(updatedId.Value, cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate(
                "program_version_code_exists",
                "Mã phiên bản đã tồn tại trong chương trình đào tạo.",
                exception);
        }
    }

    public async Task<ProgramVersionResponse> PublishProgramVersionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        const string lockSql = """
            SELECT program_id AS ProgramId, dataset_status AS Status
            FROM curriculum.program_versions
            WHERE id = @Id AND archived_at IS NULL
            FOR UPDATE;
            """;
        const string countCoursesSql = """
            SELECT COUNT(*)
            FROM curriculum.program_courses
            WHERE program_version_id = @Id AND archived_at IS NULL;
            """;
        const string clearCurrentSql = """
            UPDATE curriculum.program_versions
            SET is_current = false, updated_at = now()
            WHERE program_id = @ProgramId AND is_current;
            """;
        const string publishSql = """
            UPDATE curriculum.program_versions
            SET dataset_status = 'finalized', is_current = true,
                published_at = now(), updated_at = now()
            WHERE id = @Id;

            UPDATE curriculum.program_courses
            SET dataset_status = 'finalized', updated_at = now()
            WHERE program_version_id = @Id AND archived_at IS NULL;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var version = await connection.QuerySingleOrDefaultAsync<VersionStateRow>(
            new CommandDefinition(lockSql, new { Id = id }, transaction, cancellationToken: cancellationToken));
        if (version is null)
        {
            throw new CurriculumNotFoundException("phiên bản chương trình đào tạo", id);
        }

        if (version.Status != CurriculumStatuses.Draft)
        {
            throw new CurriculumConflictException(
                "version_not_editable",
                "Chỉ phiên bản draft mới có thể được công bố.");
        }

        var courseCount = await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(countCoursesSql, new { Id = id }, transaction, cancellationToken: cancellationToken));
        if (courseCount == 0)
        {
            throw new CurriculumConflictException(
                "version_has_no_courses",
                "Phiên bản phải có ít nhất một học phần trước khi công bố.");
        }

        await connection.ExecuteAsync(
            new CommandDefinition(
                clearCurrentSql,
                new { version.ProgramId },
                transaction,
                cancellationToken: cancellationToken));
        await connection.ExecuteAsync(
            new CommandDefinition(publishSql, new { Id = id }, transaction, cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return (await GetProgramVersionAsync(id, cancellationToken))!;
    }

    public async Task<bool> ArchiveProgramVersionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        const string lockSql = """
            SELECT program_id AS ProgramId, is_current AS IsCurrent
            FROM curriculum.program_versions
            WHERE id = @Id AND archived_at IS NULL
            FOR UPDATE;
            """;
        const string archiveSql = """
            UPDATE curriculum.program_versions
            SET dataset_status = 'archived', is_current = false,
                archived_at = now(), updated_at = now()
            WHERE id = @Id;
            """;
        const string selectReplacementSql = """
            SELECT id
            FROM curriculum.program_versions
            WHERE program_id = @ProgramId
              AND id <> @Id
              AND dataset_status = 'finalized'
              AND archived_at IS NULL
            ORDER BY published_at DESC NULLS LAST, id DESC
            LIMIT 1;
            """;
        const string setReplacementSql = """
            UPDATE curriculum.program_versions
            SET is_current = true, updated_at = now()
            WHERE id = @ReplacementId;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var version = await connection.QuerySingleOrDefaultAsync<VersionArchiveRow>(
            new CommandDefinition(lockSql, new { Id = id }, transaction, cancellationToken: cancellationToken));
        if (version is null)
        {
            return false;
        }

        await connection.ExecuteAsync(
            new CommandDefinition(archiveSql, new { Id = id }, transaction, cancellationToken: cancellationToken));

        if (version.IsCurrent)
        {
            var replacementId = await connection.QuerySingleOrDefaultAsync<long?>(
                new CommandDefinition(
                    selectReplacementSql,
                    new { version.ProgramId, Id = id },
                    transaction,
                    cancellationToken: cancellationToken));
            if (replacementId is not null)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        setReplacementSql,
                        new { ReplacementId = replacementId.Value },
                        transaction,
                        cancellationToken: cancellationToken));
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<ProgramCourseResponse>> GetProgramCoursesAsync(
        long versionId,
        int page,
        int pageSize,
        string? search,
        string? semester,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        const string filters = """
            pc.program_version_id = @VersionId
            AND (@IncludeArchived OR pc.archived_at IS NULL)
            AND (@Search IS NULL OR pc.course_name_snapshot ILIKE '%' || @Search || '%'
                 OR pc.course_code_snapshot ILIKE '%' || @Search || '%')
            AND (@Semester IS NULL OR pc.semester_source_value ILIKE @Semester)
            """;
        var sql = "SELECT COUNT(*) FROM curriculum.program_courses pc WHERE " + filters + ";\n"
            + CourseProjection + " WHERE " + filters + """
              ORDER BY pc.display_order, pc.id
              LIMIT @PageSize OFFSET @Offset;
              """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new
            {
                VersionId = versionId,
                IncludeArchived = includeArchived,
                Search = search,
                Semester = semester,
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            },
            cancellationToken: cancellationToken);
        using var result = await connection.QueryMultipleAsync(command);
        var total = await result.ReadSingleAsync<long>();
        var items = (await result.ReadAsync<ProgramCourseResponse>()).AsList();
        return PagedResult<ProgramCourseResponse>.Create(items, page, pageSize, total);
    }

    public async Task<ProgramCourseResponse?> GetProgramCourseAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await GetProgramCourseAsync(
            connection,
            null,
            versionId,
            programCourseId,
            cancellationToken);
    }

    public async Task<ProgramCourseResponse> CreateProgramCourseAsync(
        long versionId,
        NormalizedCourse course,
        CancellationToken cancellationToken)
    {
        const string nextOrderSql = """
            SELECT COALESCE(MAX(display_order), 0) + 1
            FROM curriculum.program_courses
            WHERE program_version_id = @VersionId;
            """;
        const string insertSql = """
            INSERT INTO curriculum.program_courses
                (program_version_id, course_id, source_row, course_code_source_value,
                 credits, semester_source_value, dataset_status, quality_flags,
                 course_code_snapshot, course_name_snapshot, display_order)
            VALUES (@VersionId, @CourseId, NULL, NULL,
                    @Credits, @Semester, 'draft', '[]'::jsonb,
                    @InstitutionalCode, @Name, @DisplayOrder)
            RETURNING id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
            var courseId = await GetOrCreateCourseIdAsync(
                connection,
                transaction,
                course.InstitutionalCode,
                course.Name,
                cancellationToken);
            var displayOrder = course.DisplayOrder ?? await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    nextOrderSql,
                    new { VersionId = versionId },
                    transaction,
                    cancellationToken: cancellationToken));
            var programCourseId = await connection.QuerySingleAsync<long>(
                new CommandDefinition(
                    insertSql,
                    new
                    {
                        VersionId = versionId,
                        CourseId = courseId,
                        course.Credits,
                        course.Semester,
                        course.InstitutionalCode,
                        course.Name,
                        DisplayOrder = displayOrder
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return (await GetProgramCourseAsync(versionId, programCourseId, cancellationToken))!;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw CourseDuplicate(exception);
        }
    }

    public async Task<ProgramCourseResponse?> UpdateProgramCourseAsync(
        long versionId,
        long programCourseId,
        NormalizedCourse course,
        CancellationToken cancellationToken)
    {
        const string currentCourseSql = """
            SELECT course_id
            FROM curriculum.program_courses
            WHERE id = @ProgramCourseId
              AND program_version_id = @VersionId
              AND archived_at IS NULL
            FOR UPDATE;
            """;
        const string updateSql = """
            UPDATE curriculum.program_courses
            SET course_id = @CourseId,
                course_code_snapshot = @InstitutionalCode,
                course_name_snapshot = @Name,
                credits = @Credits,
                semester_source_value = @Semester,
                display_order = @DisplayOrder,
                updated_at = now()
            WHERE id = @ProgramCourseId
              AND program_version_id = @VersionId
              AND archived_at IS NULL
            RETURNING id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
            var currentCourseId = await connection.QuerySingleOrDefaultAsync<long?>(
                new CommandDefinition(
                    currentCourseSql,
                    new { VersionId = versionId, ProgramCourseId = programCourseId },
                    transaction,
                    cancellationToken: cancellationToken));
            if (currentCourseId is null)
            {
                return null;
            }

            var courseId = course.InstitutionalCode is null
                ? currentCourseId.Value
                : await GetOrCreateCourseIdAsync(
                    connection,
                    transaction,
                    course.InstitutionalCode,
                    course.Name,
                    cancellationToken);
            await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        VersionId = versionId,
                        ProgramCourseId = programCourseId,
                        CourseId = courseId,
                        course.InstitutionalCode,
                        course.Name,
                        course.Credits,
                        course.Semester,
                        DisplayOrder = course.DisplayOrder!.Value
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return await GetProgramCourseAsync(versionId, programCourseId, cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw CourseDuplicate(exception);
        }
    }

    public async Task<bool> ArchiveProgramCourseAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE curriculum.program_courses
            SET dataset_status = 'archived', archived_at = now(), updated_at = now()
            WHERE id = @ProgramCourseId
              AND program_version_id = @VersionId
              AND archived_at IS NULL;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { VersionId = versionId, ProgramCourseId = programCourseId },
                transaction,
                cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return affected == 1;
    }

    private static async Task<ProgramVersionResponse?> GetProgramVersionAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        long id,
        CancellationToken cancellationToken)
    {
        var sql = VersionProjection + " WHERE pv.id = @Id;";
        return await connection.QuerySingleOrDefaultAsync<ProgramVersionResponse>(
            new CommandDefinition(sql, new { Id = id }, transaction, cancellationToken: cancellationToken));
    }

    private static async Task<ProgramCourseResponse?> GetProgramCourseAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        var sql = CourseProjection + """

            WHERE pc.id = @ProgramCourseId AND pc.program_version_id = @VersionId;
            """;
        return await connection.QuerySingleOrDefaultAsync<ProgramCourseResponse>(
            new CommandDefinition(
                sql,
                new { VersionId = versionId, ProgramCourseId = programCourseId },
                transaction,
                cancellationToken: cancellationToken));
    }

    private static async Task EnsureDraftVersionAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long versionId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT dataset_status
            FROM curriculum.program_versions
            WHERE id = @VersionId AND archived_at IS NULL
            FOR UPDATE;
            """;
        var status = await connection.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                sql,
                new { VersionId = versionId },
                transaction,
                cancellationToken: cancellationToken));
        if (status is null)
        {
            throw new CurriculumNotFoundException("phiên bản chương trình đào tạo", versionId);
        }

        if (status != CurriculumStatuses.Draft)
        {
            throw new CurriculumConflictException(
                "version_not_editable",
                "Chỉ phiên bản ở trạng thái draft mới được chỉnh sửa.");
        }
    }

    private static async Task<long> GetOrCreateCourseIdAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string? institutionalCode,
        string name,
        CancellationToken cancellationToken)
    {
        if (institutionalCode is not null)
        {
            const string findSql = """
                SELECT id FROM curriculum.courses WHERE institutional_code = @InstitutionalCode;
                """;
            var existingId = await connection.QuerySingleOrDefaultAsync<long?>(
                new CommandDefinition(
                    findSql,
                    new { InstitutionalCode = institutionalCode },
                    transaction,
                    cancellationToken: cancellationToken));
            if (existingId is not null)
            {
                return existingId.Value;
            }
        }

        const string insertSql = """
            INSERT INTO curriculum.courses (institutional_code, name)
            VALUES (@InstitutionalCode, @Name)
            RETURNING id;
            """;
        return await connection.QuerySingleAsync<long>(
            new CommandDefinition(
                insertSql,
                new { InstitutionalCode = institutionalCode, Name = name },
                transaction,
                cancellationToken: cancellationToken));
    }

    private static CurriculumConflictException Duplicate(
        string code,
        string message,
        Exception innerException)
    {
        _ = innerException;
        return new CurriculumConflictException(code, message);
    }

    private static CurriculumConflictException CourseDuplicate(PostgresException exception)
    {
        return exception.ConstraintName switch
        {
            "program_courses_active_course_uq" => new CurriculumConflictException(
                "course_already_in_version",
                "Học phần đã tồn tại trong phiên bản chương trình đào tạo."),
            _ => new CurriculumConflictException(
                "course_code_exists",
                "Mã học phần đã tồn tại trong phiên bản chương trình đào tạo.")
        };
    }

    private sealed record VersionStateRow(long ProgramId, string Status);

    private sealed record VersionArchiveRow(long ProgramId, bool IsCurrent);
}
