using Dapper;
using Npgsql;
using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

public sealed partial class NpgsqlCurriculumRepository
{
    private const string PloProjection = """
        SELECT p.id AS Id,
               p.program_version_id AS ProgramVersionId,
               p.code AS Code,
               p.statement AS Statement,
               p.level_code AS LevelCode,
               p.provenance AS Provenance
        FROM curriculum.plos p
        """;

    private const string CloProjection = """
        SELECT c.id AS Id,
               c.program_course_id AS ProgramCourseId,
               c.code AS Code,
               c.statement AS Statement,
               c.level_code AS LevelCode,
               c.provenance AS Provenance,
               c.dataset_status AS Status
        FROM curriculum.clos c
        JOIN curriculum.program_courses pc ON pc.id = c.program_course_id
        """;

    private const string CoursePloProjection = """
        SELECT cp.id AS Id,
               cp.program_version_id AS ProgramVersionId,
               cp.program_course_id AS ProgramCourseId,
               cp.plo_id AS PloId,
               p.code AS PloCode,
               cp.weight_code::text AS WeightCode,
               cp.progression_code::text AS ProgressionCode,
               cp.provenance AS Provenance,
               cp.fit AS Fit,
               cp.fit_reason AS FitReason
        FROM curriculum.course_plos cp
        JOIN curriculum.plos p ON p.id = cp.plo_id
        """;

    private const string CloPloProjection = """
        SELECT x.clo_id AS CloId,
               c.code AS CloCode,
               x.course_plo_id AS CoursePloId,
               cp.plo_id AS PloId,
               p.code AS PloCode
        FROM curriculum.clo_plos x
        JOIN curriculum.clos c ON c.id = x.clo_id
        JOIN curriculum.course_plos cp ON cp.id = x.course_plo_id
        JOIN curriculum.plos p ON p.id = cp.plo_id
        """;

    public async Task<IReadOnlyList<PloResponse>> GetPlosAsync(
        long versionId,
        CancellationToken cancellationToken)
    {
        var sql = PloProjection + """

            WHERE p.program_version_id = @VersionId
            ORDER BY p.code, p.id;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<PloResponse>(
            new CommandDefinition(sql, new { VersionId = versionId }, cancellationToken: cancellationToken));
        return items.AsList();
    }

    public async Task<PloResponse?> GetPloAsync(
        long versionId,
        long ploId,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await GetPloAsync(connection, null, versionId, ploId, cancellationToken);
    }

    public async Task<PloResponse> CreatePloAsync(
        long versionId,
        NormalizedLearningOutcome plo,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO curriculum.plos
                (program_version_id, code, statement, level_code, provenance)
            VALUES (@VersionId, @Code, @Statement, @LevelCode, 'manual')
            RETURNING id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
            var id = await connection.QuerySingleAsync<long>(new CommandDefinition(
                sql,
                new { VersionId = versionId, plo.Code, plo.Statement, plo.LevelCode },
                transaction,
                cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return (await GetPloAsync(versionId, id, cancellationToken))!;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate("plo_code_exists", "Mã PLO đã tồn tại trong phiên bản.", exception);
        }
    }

    public async Task<PloResponse?> UpdatePloAsync(
        long versionId,
        long ploId,
        NormalizedLearningOutcome plo,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE curriculum.plos
            SET code = @Code, statement = @Statement, level_code = @LevelCode,
                provenance = 'manual'
            WHERE id = @PloId AND program_version_id = @VersionId
            RETURNING id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
            var id = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(
                sql,
                new { VersionId = versionId, PloId = ploId, plo.Code, plo.Statement, plo.LevelCode },
                transaction,
                cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return id is null ? null : await GetPloAsync(versionId, id.Value, cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate("plo_code_exists", "Mã PLO đã tồn tại trong phiên bản.", exception);
        }
    }

    public async Task<bool> DeletePloAsync(
        long versionId,
        long ploId,
        CancellationToken cancellationToken)
    {
        const string usedSql = """
            SELECT EXISTS (
                SELECT 1 FROM curriculum.course_plos
                WHERE plo_id = @PloId AND program_version_id = @VersionId
            );
            """;
        const string deleteSql = """
            DELETE FROM curriculum.plos
            WHERE id = @PloId AND program_version_id = @VersionId;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
        if (await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                usedSql,
                new { VersionId = versionId, PloId = ploId },
                transaction,
                cancellationToken: cancellationToken)))
        {
            throw new CurriculumConflictException(
                "plo_has_course_mappings",
                "Không thể xóa PLO đang được ánh xạ với học phần.");
        }

        var affected = await connection.ExecuteAsync(new CommandDefinition(
            deleteSql,
            new { VersionId = versionId, PloId = ploId },
            transaction,
            cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return affected == 1;
    }

    public async Task<IReadOnlyList<CloResponse>> GetClosAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        var sql = CloProjection + """

            WHERE pc.program_version_id = @VersionId
              AND pc.id = @ProgramCourseId
            ORDER BY c.code, c.id;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<CloResponse>(new CommandDefinition(
            sql,
            new { VersionId = versionId, ProgramCourseId = programCourseId },
            cancellationToken: cancellationToken));
        return items.AsList();
    }

    public async Task<CloResponse?> GetCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await GetCloAsync(
            connection,
            null,
            versionId,
            programCourseId,
            cloId,
            cancellationToken);
    }

    public async Task<CloResponse> CreateCloAsync(
        long versionId,
        long programCourseId,
        NormalizedLearningOutcome clo,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO curriculum.clos
                (program_course_id, code, statement, level_code, provenance, dataset_status)
            SELECT pc.id, @Code, @Statement, @LevelCode, 'manual', 'draft'
            FROM curriculum.program_courses pc
            WHERE pc.id = @ProgramCourseId
              AND pc.program_version_id = @VersionId
              AND pc.archived_at IS NULL
            RETURNING id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
            var id = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(
                sql,
                new
                {
                    VersionId = versionId,
                    ProgramCourseId = programCourseId,
                    clo.Code,
                    clo.Statement,
                    clo.LevelCode
                },
                transaction,
                cancellationToken: cancellationToken));
            if (id is null)
            {
                throw new CurriculumNotFoundException("học phần trong phiên bản", programCourseId);
            }

            await transaction.CommitAsync(cancellationToken);
            return (await GetCloAsync(versionId, programCourseId, id.Value, cancellationToken))!;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate("clo_code_exists", "Mã CLO đã tồn tại trong học phần.", exception);
        }
    }

    public async Task<CloResponse?> UpdateCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        NormalizedLearningOutcome clo,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE curriculum.clos c
            SET code = @Code, statement = @Statement, level_code = @LevelCode,
                provenance = 'manual', dataset_status = 'draft'
            FROM curriculum.program_courses pc
            WHERE c.id = @CloId
              AND c.program_course_id = pc.id
              AND pc.id = @ProgramCourseId
              AND pc.program_version_id = @VersionId
              AND pc.archived_at IS NULL
            RETURNING c.id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
            var id = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(
                sql,
                new
                {
                    VersionId = versionId,
                    ProgramCourseId = programCourseId,
                    CloId = cloId,
                    clo.Code,
                    clo.Statement,
                    clo.LevelCode
                },
                transaction,
                cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return id is null
                ? null
                : await GetCloAsync(versionId, programCourseId, id.Value, cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate("clo_code_exists", "Mã CLO đã tồn tại trong học phần.", exception);
        }
    }

    public async Task<bool> DeleteCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken)
    {
        const string findSql = """
            SELECT c.id
            FROM curriculum.clos c
            JOIN curriculum.program_courses pc ON pc.id = c.program_course_id
            WHERE c.id = @CloId
              AND pc.id = @ProgramCourseId
              AND pc.program_version_id = @VersionId
              AND pc.archived_at IS NULL
            FOR UPDATE OF c;
            """;
        const string deleteMappingsSql = "DELETE FROM curriculum.clo_plos WHERE clo_id = @CloId;";
        const string deleteSql = "DELETE FROM curriculum.clos WHERE id = @CloId;";

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
        var id = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(
            findSql,
            new { VersionId = versionId, ProgramCourseId = programCourseId, CloId = cloId },
            transaction,
            cancellationToken: cancellationToken));
        if (id is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        await connection.ExecuteAsync(new CommandDefinition(
            deleteMappingsSql,
            new { CloId = cloId },
            transaction,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            deleteSql,
            new { CloId = cloId },
            transaction,
            cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<CoursePloMappingResponse>> GetCoursePloMappingsAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        var sql = CoursePloProjection + """

            WHERE cp.program_version_id = @VersionId
              AND cp.program_course_id = @ProgramCourseId
            ORDER BY p.code, cp.id;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<CoursePloMappingResponse>(new CommandDefinition(
            sql,
            new { VersionId = versionId, ProgramCourseId = programCourseId },
            cancellationToken: cancellationToken));
        return items.AsList();
    }

    public async Task<CoursePloMappingResponse> CreateCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        NormalizedCoursePloMapping mapping,
        CancellationToken cancellationToken)
    {
        const string resourcesSql = """
            SELECT EXISTS (
                       SELECT 1 FROM curriculum.program_courses
                       WHERE id = @ProgramCourseId AND program_version_id = @VersionId
                         AND archived_at IS NULL
                   ) AS CourseExists,
                   EXISTS (
                       SELECT 1 FROM curriculum.plos
                       WHERE id = @PloId AND program_version_id = @VersionId
                   ) AS PloExists;
            """;
        const string insertSql = """
            INSERT INTO curriculum.course_plos
                (program_version_id, program_course_id, plo_id, weight_code,
                 progression_code, provenance, fit, fit_reason)
            VALUES (@VersionId, @ProgramCourseId, @PloId, @WeightCode,
                    @ProgressionCode, 'manual', @Fit, @FitReason)
            RETURNING id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
            var resources = await connection.QuerySingleAsync<MappingResources>(new CommandDefinition(
                resourcesSql,
                new { VersionId = versionId, ProgramCourseId = programCourseId, mapping.PloId },
                transaction,
                cancellationToken: cancellationToken));
            if (!resources.CourseExists)
            {
                throw new CurriculumNotFoundException("học phần trong phiên bản", programCourseId);
            }

            if (!resources.PloExists)
            {
                throw new CurriculumNotFoundException("PLO", mapping.PloId);
            }

            var id = await connection.QuerySingleAsync<long>(new CommandDefinition(
                insertSql,
                new
                {
                    VersionId = versionId,
                    ProgramCourseId = programCourseId,
                    mapping.PloId,
                    mapping.WeightCode,
                    mapping.ProgressionCode,
                    mapping.Fit,
                    mapping.FitReason
                },
                transaction,
                cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return (await GetCoursePloMappingAsync(
                versionId,
                programCourseId,
                id,
                cancellationToken))!;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate(
                "course_plo_mapping_exists",
                "Học phần đã được ánh xạ với PLO này.",
                exception);
        }
    }

    public async Task<CoursePloMappingResponse?> UpdateCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        NormalizedCoursePloMappingUpdate mapping,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE curriculum.course_plos
            SET weight_code = @WeightCode,
                progression_code = @ProgressionCode,
                provenance = 'manual',
                fit = @Fit,
                fit_reason = @FitReason
            WHERE id = @MappingId
              AND program_version_id = @VersionId
              AND program_course_id = @ProgramCourseId
            RETURNING id;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
        var id = await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(
            sql,
            new
            {
                VersionId = versionId,
                ProgramCourseId = programCourseId,
                MappingId = mappingId,
                mapping.WeightCode,
                mapping.ProgressionCode,
                mapping.Fit,
                mapping.FitReason
            },
            transaction,
            cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return id is null
            ? null
            : await GetCoursePloMappingAsync(versionId, programCourseId, id.Value, cancellationToken);
    }

    public async Task<bool> DeleteCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        CancellationToken cancellationToken)
    {
        const string usedSql = "SELECT EXISTS (SELECT 1 FROM curriculum.clo_plos WHERE course_plo_id = @MappingId);";
        const string deleteSql = """
            DELETE FROM curriculum.course_plos
            WHERE id = @MappingId
              AND program_version_id = @VersionId
              AND program_course_id = @ProgramCourseId;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
        if (await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                usedSql,
                new { MappingId = mappingId },
                transaction,
                cancellationToken: cancellationToken)))
        {
            throw new CurriculumConflictException(
                "course_plo_has_clo_mappings",
                "Không thể xóa mapping học phần-PLO đang được CLO sử dụng.");
        }

        var affected = await connection.ExecuteAsync(new CommandDefinition(
            deleteSql,
            new { VersionId = versionId, ProgramCourseId = programCourseId, MappingId = mappingId },
            transaction,
            cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return affected == 1;
    }

    public async Task<IReadOnlyList<CloPloMappingResponse>> GetCloPloMappingsAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken)
    {
        var sql = CloPloProjection + """

            WHERE x.clo_id = @CloId
              AND x.program_course_id = @ProgramCourseId
              AND cp.program_version_id = @VersionId
            ORDER BY p.code, cp.id;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<CloPloMappingResponse>(new CommandDefinition(
            sql,
            new { VersionId = versionId, ProgramCourseId = programCourseId, CloId = cloId },
            cancellationToken: cancellationToken));
        return items.AsList();
    }

    public async Task<CloPloMappingResponse> CreateCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        long coursePloId,
        CancellationToken cancellationToken)
    {
        const string resourcesSql = """
            SELECT EXISTS (
                       SELECT 1 FROM curriculum.clos c
                       JOIN curriculum.program_courses pc ON pc.id = c.program_course_id
                       WHERE c.id = @CloId AND c.program_course_id = @ProgramCourseId
                         AND pc.program_version_id = @VersionId AND pc.archived_at IS NULL
                   ) AS CloExists,
                   EXISTS (
                       SELECT 1 FROM curriculum.course_plos
                       WHERE id = @CoursePloId AND program_course_id = @ProgramCourseId
                         AND program_version_id = @VersionId
                   ) AS CoursePloExists;
            """;
        const string insertSql = """
            INSERT INTO curriculum.clo_plos (clo_id, program_course_id, course_plo_id)
            VALUES (@CloId, @ProgramCourseId, @CoursePloId);
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
            var resources = await connection.QuerySingleAsync<CloMappingResources>(new CommandDefinition(
                resourcesSql,
                new
                {
                    VersionId = versionId,
                    ProgramCourseId = programCourseId,
                    CloId = cloId,
                    CoursePloId = coursePloId
                },
                transaction,
                cancellationToken: cancellationToken));
            if (!resources.CloExists)
            {
                throw new CurriculumNotFoundException("CLO", cloId);
            }

            if (!resources.CoursePloExists)
            {
                throw new CurriculumNotFoundException("mapping học phần-PLO", coursePloId);
            }

            await connection.ExecuteAsync(new CommandDefinition(
                insertSql,
                new { ProgramCourseId = programCourseId, CloId = cloId, CoursePloId = coursePloId },
                transaction,
                cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return (await GetCloPloMappingAsync(
                versionId,
                programCourseId,
                cloId,
                coursePloId,
                cancellationToken))!;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Duplicate("clo_plo_mapping_exists", "CLO đã được ánh xạ với PLO này.", exception);
        }
    }

    public async Task<bool> DeleteCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        long coursePloId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM curriculum.clo_plos x
            USING curriculum.course_plos cp
            WHERE x.clo_id = @CloId
              AND x.program_course_id = @ProgramCourseId
              AND x.course_plo_id = @CoursePloId
              AND cp.id = x.course_plo_id
              AND cp.program_version_id = @VersionId;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await EnsureDraftVersionAsync(connection, transaction, versionId, cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                VersionId = versionId,
                ProgramCourseId = programCourseId,
                CloId = cloId,
                CoursePloId = coursePloId
            },
            transaction,
            cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return affected == 1;
    }

    public async Task<IReadOnlyList<CoursePloCreditCheckResponse>> GetPloCreditChecksAsync(
        long versionId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT pc.id AS ProgramCourseId,
                   pc.course_code_snapshot AS InstitutionalCode,
                   pc.course_name_snapshot AS CourseName,
                   pc.credits AS Credits,
                   LEAST(pc.credits, 5) AS RequiredPloCount,
                   COUNT(cp.id)::integer AS ActualPloCount,
                   COUNT(cp.id) >= LEAST(pc.credits, 5) AS IsValid
            FROM curriculum.program_courses pc
            LEFT JOIN curriculum.course_plos cp ON cp.program_course_id = pc.id
            WHERE pc.program_version_id = @VersionId
              AND pc.archived_at IS NULL
            GROUP BY pc.id
            ORDER BY pc.display_order, pc.id;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<CoursePloCreditCheckResponse>(new CommandDefinition(
            sql,
            new { VersionId = versionId },
            cancellationToken: cancellationToken));
        return items.AsList();
    }

    public async Task<IReadOnlyList<PloBalanceItemResponse>> GetPloBalanceAsync(
        long versionId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH scores AS (
                SELECT p.id, p.code AS PloCode,
                       COALESCE(SUM(pc.credits * w.score), 0)::numeric AS Score
                FROM curriculum.plos p
                LEFT JOIN curriculum.course_plos cp ON cp.plo_id = p.id
                LEFT JOIN curriculum.program_courses pc
                  ON pc.id = cp.program_course_id AND pc.archived_at IS NULL
                LEFT JOIN curriculum.weight_codes w ON w.code = cp.weight_code
                WHERE p.program_version_id = @VersionId
                GROUP BY p.id, p.code
            ), balance AS (
                SELECT scores.*, AVG(Score) OVER () AS MeanScore
                FROM scores
            )
            SELECT PloCode,
                   Score,
                   MeanScore,
                   ROUND((Score - MeanScore) / NULLIF(MeanScore, 0), 4) AS Deviation,
                   COALESCE(ABS((Score - MeanScore) / NULLIF(MeanScore, 0)) > 0.20, false)
                       AS ExceedsTwentyPercent
            FROM balance
            ORDER BY PloCode, id;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<PloBalanceItemResponse>(new CommandDefinition(
            sql,
            new { VersionId = versionId },
            cancellationToken: cancellationToken));
        return items.AsList();
    }

    private static async Task<PloResponse?> GetPloAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        long versionId,
        long ploId,
        CancellationToken cancellationToken)
    {
        var sql = PloProjection + " WHERE p.id = @PloId AND p.program_version_id = @VersionId;";
        return await connection.QuerySingleOrDefaultAsync<PloResponse>(new CommandDefinition(
            sql,
            new { VersionId = versionId, PloId = ploId },
            transaction,
            cancellationToken: cancellationToken));
    }

    private static async Task<CloResponse?> GetCloAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken)
    {
        var sql = CloProjection + """

            WHERE c.id = @CloId
              AND pc.id = @ProgramCourseId
              AND pc.program_version_id = @VersionId;
            """;
        return await connection.QuerySingleOrDefaultAsync<CloResponse>(new CommandDefinition(
            sql,
            new { VersionId = versionId, ProgramCourseId = programCourseId, CloId = cloId },
            transaction,
            cancellationToken: cancellationToken));
    }

    private async Task<CoursePloMappingResponse?> GetCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        CancellationToken cancellationToken)
    {
        var sql = CoursePloProjection + """

            WHERE cp.id = @MappingId
              AND cp.program_version_id = @VersionId
              AND cp.program_course_id = @ProgramCourseId;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<CoursePloMappingResponse>(new CommandDefinition(
            sql,
            new { VersionId = versionId, ProgramCourseId = programCourseId, MappingId = mappingId },
            cancellationToken: cancellationToken));
    }

    private async Task<CloPloMappingResponse?> GetCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        long coursePloId,
        CancellationToken cancellationToken)
    {
        var sql = CloPloProjection + """

            WHERE x.clo_id = @CloId
              AND x.program_course_id = @ProgramCourseId
              AND x.course_plo_id = @CoursePloId
              AND cp.program_version_id = @VersionId;
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<CloPloMappingResponse>(new CommandDefinition(
            sql,
            new
            {
                VersionId = versionId,
                ProgramCourseId = programCourseId,
                CloId = cloId,
                CoursePloId = coursePloId
            },
            cancellationToken: cancellationToken));
    }

    private sealed record MappingResources(bool CourseExists, bool PloExists);

    private sealed record CloMappingResources(bool CloExists, bool CoursePloExists);
}
