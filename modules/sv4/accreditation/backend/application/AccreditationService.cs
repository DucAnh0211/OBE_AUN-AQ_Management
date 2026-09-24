using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;

namespace ObeAunQa.Modules.Accreditation.Application;

public sealed class AccreditationService(NpgsqlDataSource dataSource)
{
    public async Task<IReadOnlyList<FrameworkSummary>> GetFrameworksAsync(string? status, string? search, CancellationToken ct)
    {
        const string sql = """
        SELECT f.id AS "Id", f.code AS "Code", f.version AS "Version",
               f.assessment_level AS "AssessmentLevel", f.default_language AS "DefaultLanguage",
               f.status AS "Status", f.source_title AS "SourceTitle", f.source_url AS "SourceUrl",
               COUNT(DISTINCT c.id)::int AS "CriterionCount", COUNT(DISTINCT r.id)::int AS "RequirementCount",
               f.published_at AS "PublishedAt", f.retired_at AS "RetiredAt"
        FROM accreditation.aun_frameworks f
        LEFT JOIN accreditation.aun_criteria c ON c.framework_id=f.id
        LEFT JOIN accreditation.aun_requirements r ON r.criterion_id=c.id
        WHERE (@Status IS NULL OR f.status=@Status)
          AND (@Search IS NULL OR f.code ILIKE '%'||@Search||'%' OR f.version ILIKE '%'||@Search||'%' OR f.source_title ILIKE '%'||@Search||'%')
        GROUP BY f.id ORDER BY f.created_at DESC;
        """;
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<FrameworkSummary>(new CommandDefinition(sql,
            new { Status = Clean(status), Search = Clean(search) }, cancellationToken: ct))).AsList();
    }

    public async Task<FrameworkSummary> GetFrameworkAsync(long id, CancellationToken ct)
        => (await GetFrameworksAsync(null, null, ct)).FirstOrDefault(x => x.Id == id)
           ?? throw new AccreditationNotFoundException("Không tìm thấy framework.");

    public async Task<FrameworkTree> GetTreeAsync(long id, string? language, CancellationToken ct)
    {
        var framework = await GetFrameworkAsync(id, ct);
        var requested = Clean(language) ?? framework.DefaultLanguage;
        const string sql = """
        SELECT c.id AS "CriterionId", c.code AS "CriterionCode", c.display_order AS "CriterionOrder",
               COALESCE(ct.title, cf.title, ce.title, c.code) AS "CriterionTitle",
               CASE WHEN ct.title IS NOT NULL THEN @Language WHEN cf.title IS NOT NULL THEN @DefaultLanguage WHEN ce.title IS NOT NULL THEN 'en' ELSE 'none' END AS "CriterionLanguage",
               CASE WHEN ct.title IS NOT NULL THEN ct.translation_status WHEN cf.title IS NOT NULL THEN 'fallback_'||@DefaultLanguage WHEN ce.title IS NOT NULL THEN 'fallback_en' ELSE 'missing' END AS "CriterionTranslationStatus",
               r.id AS "RequirementId", r.code AS "RequirementCode", r.display_order AS "RequirementOrder", r.source_page AS "SourcePage",
               COALESCE(rt.statement, rf.statement, re.statement, r.code) AS "Statement",
               CASE WHEN rt.statement IS NOT NULL THEN @Language WHEN rf.statement IS NOT NULL THEN @DefaultLanguage WHEN re.statement IS NOT NULL THEN 'en' ELSE 'none' END AS "RequirementLanguage",
               CASE WHEN rt.statement IS NOT NULL THEN rt.translation_status WHEN rf.statement IS NOT NULL THEN 'fallback_'||@DefaultLanguage WHEN re.statement IS NOT NULL THEN 'fallback_en' ELSE 'missing' END AS "RequirementTranslationStatus"
        FROM accreditation.aun_criteria c
        LEFT JOIN accreditation.aun_criterion_translations ct ON ct.criterion_id=c.id AND ct.language_code=@Language
        LEFT JOIN accreditation.aun_criterion_translations cf ON cf.criterion_id=c.id AND cf.language_code=@DefaultLanguage
        LEFT JOIN accreditation.aun_criterion_translations ce ON ce.criterion_id=c.id AND ce.language_code='en'
        LEFT JOIN accreditation.aun_requirements r ON r.criterion_id=c.id
        LEFT JOIN accreditation.aun_requirement_translations rt ON rt.requirement_id=r.id AND rt.language_code=@Language
        LEFT JOIN accreditation.aun_requirement_translations rf ON rf.requirement_id=r.id AND rf.language_code=@DefaultLanguage
        LEFT JOIN accreditation.aun_requirement_translations re ON re.requirement_id=r.id AND re.language_code='en'
        WHERE c.framework_id=@Id ORDER BY c.display_order,r.display_order;
        """;
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var rows = (await connection.QueryAsync<TreeRow>(new CommandDefinition(sql,
            new { Id = id, Language = requested, DefaultLanguage = framework.DefaultLanguage }, cancellationToken: ct))).AsList();
        var criteria = rows.GroupBy(x => new { x.CriterionId, x.CriterionCode, x.CriterionOrder, x.CriterionTitle, x.CriterionLanguage, x.CriterionTranslationStatus })
            .Select(g => new CriterionTree(g.Key.CriterionId, g.Key.CriterionCode, g.Key.CriterionOrder,
                g.Key.CriterionTitle, g.Key.CriterionLanguage, g.Key.CriterionTranslationStatus,
                g.Where(x => x.RequirementId.HasValue).Select(x => new RequirementTree(x.RequirementId!.Value,
                    x.RequirementCode!, x.RequirementOrder!.Value, x.SourcePage, x.Statement!, x.RequirementLanguage!, x.RequirementTranslationStatus!)).ToList())).ToList();
        return new FrameworkTree(framework.Id, framework.Code, framework.Version, framework.Status,
            requested, framework.DefaultLanguage, framework.CriterionCount, framework.RequirementCount, criteria);
    }

    public ImportValidationResult PreviewImport(FrameworkImportRequest request) => FrameworkValidator.Validate(request);

    public async Task<long> ImportAsync(FrameworkImportRequest request, CancellationToken ct)
    {
        var result = FrameworkValidator.Validate(request);
        if (!result.Valid) throw new AccreditationValidationException("Framework import không hợp lệ.", result.Errors);
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        try
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)))).ToLowerInvariant();
            const string insertFramework = """
            INSERT INTO accreditation.aun_frameworks(code,version,assessment_level,default_language,status,source_title,source_url,content_hash)
            VALUES(@Code,@Version,@Level,@Language,'draft',@SourceTitle,@SourceUrl,@Hash) RETURNING id;
            """;
            long id;
            try { id = await connection.ExecuteScalarAsync<long>(new CommandDefinition(insertFramework, new
                { Code=request.Code!.Trim(), Version=request.Version!.Trim(), Level=Clean(request.AssessmentLevel)??"programme", Language=Clean(request.DefaultLanguage)??"vi", request.SourceTitle, request.SourceUrl, Hash=hash }, tx, cancellationToken:ct)); }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            { throw new AccreditationConflictException("framework_version_exists", "Framework và phiên bản đã tồn tại."); }
            foreach (var criterion in request.Criteria!)
            {
                var criterionId = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
                    "INSERT INTO accreditation.aun_criteria(framework_id,code,display_order) VALUES(@FrameworkId,@Code,@Order) RETURNING id",
                    new { FrameworkId=id, Code=criterion.Code!.Trim(), Order=criterion.DisplayOrder }, tx, cancellationToken:ct));
                await SaveCriterionTranslations(connection, tx, criterionId, criterion.Translations, ct);
                foreach (var requirement in criterion.Requirements!)
                {
                    var requirementId = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
                        "INSERT INTO accreditation.aun_requirements(criterion_id,code,display_order,source_page) VALUES(@CriterionId,@Code,@Order,@Page) RETURNING id",
                        new { CriterionId=criterionId, Code=requirement.Code!.Trim(), Order=requirement.DisplayOrder, Page=requirement.SourcePage }, tx, cancellationToken:ct));
                    await SaveRequirementTranslations(connection, tx, requirementId, requirement.Translations, ct);
                }
            }
            await tx.CommitAsync(ct); return id;
        }
        catch { await tx.RollbackAsync(ct); throw; }
    }

    public async Task<long> CreateFrameworkAsync(CreateFrameworkRequest request, CancellationToken ct)
    {
        var import = new FrameworkImportRequest(request.Code, request.Version, request.AssessmentLevel,
            request.DefaultLanguage, request.SourceTitle, request.SourceUrl, []);
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Version) || string.IsNullOrWhiteSpace(request.SourceTitle))
            throw new AccreditationValidationException("Dữ liệu không hợp lệ.", new Dictionary<string,string[]> { ["framework"]=["Code, version và sourceTitle là bắt buộc."] });
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        try { return await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            "INSERT INTO accreditation.aun_frameworks(code,version,assessment_level,default_language,status,source_title,source_url) VALUES(@Code,@Version,@Level,@Language,'draft',@SourceTitle,@SourceUrl) RETURNING id",
            new { Code=request.Code.Trim(), Version=request.Version.Trim(), Level=Clean(request.AssessmentLevel)??"programme", Language=Clean(request.DefaultLanguage)??"vi", request.SourceTitle, request.SourceUrl }, cancellationToken:ct)); }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        { throw new AccreditationConflictException("framework_version_exists", "Framework và phiên bản đã tồn tại."); }
    }

    public async Task UpdateFrameworkAsync(long id, UpdateFrameworkRequest request, CancellationToken ct)
    {
        await EnsureDraft(id,ct);
        await Execute("""
            UPDATE accreditation.aun_frameworks SET default_language=@Language,
            source_title=@SourceTitle,source_url=@SourceUrl,updated_at=now() WHERE id=@Id
            """,new{Id=id,Language=Required(request.DefaultLanguage,"defaultLanguage"),SourceTitle=Required(request.SourceTitle,"sourceTitle"),request.SourceUrl},ct);
    }

    public Task<long> AddCriterionAsync(long frameworkId, CriterionRequest request, CancellationToken ct)
        => MutateDraftScalar(frameworkId,
            "INSERT INTO accreditation.aun_criteria(framework_id,code,display_order) VALUES(@FrameworkId,@Code,@Order) RETURNING id",
            new { FrameworkId=frameworkId, Code=Required(request.Code,"code"), Order=request.DisplayOrder }, ct);

    public async Task UpdateCriterionAsync(long frameworkId,long criterionId,CriterionRequest request,CancellationToken ct)
    {
        await EnsureDraft(frameworkId,ct);
        var affected=await Execute("UPDATE accreditation.aun_criteria SET code=@Code,display_order=@Order,updated_at=now() WHERE id=@Id AND framework_id=@FrameworkId",
            new{Id=criterionId,FrameworkId=frameworkId,Code=Required(request.Code,"code"),Order=request.DisplayOrder},ct);
        if(affected==0) throw new AccreditationNotFoundException("Không tìm thấy criterion.");
    }

    public async Task<long> AddRequirementAsync(long frameworkId, long criterionId, RequirementRequest request, CancellationToken ct)
    {
        await EnsureDraft(frameworkId, ct);
        var criterionCode = await ScalarOrNotFound<string>("SELECT code FROM accreditation.aun_criteria WHERE id=@Id AND framework_id=@FrameworkId", new { Id=criterionId, FrameworkId=frameworkId }, ct);
        var code = Required(request.Code,"code");
        if (!code.StartsWith(criterionCode+".", StringComparison.Ordinal)) throw new AccreditationValidationException("Mã requirement không hợp lệ.", new Dictionary<string,string[]> { ["code"]=["Mã requirement phải thuộc criterion."] });
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            "INSERT INTO accreditation.aun_requirements(criterion_id,code,display_order,source_page) VALUES(@CriterionId,@Code,@Order,@Page) RETURNING id",
            new { CriterionId=criterionId, Code=code, Order=request.DisplayOrder, Page=request.SourcePage }, cancellationToken:ct));
    }

    public async Task UpdateRequirementAsync(long frameworkId,long requirementId,RequirementRequest request,CancellationToken ct)
    {
        await EnsureDraft(frameworkId,ct);
        const string sql="""
        UPDATE accreditation.aun_requirements r SET code=@Code,display_order=@Order,source_page=@Page,updated_at=now()
        FROM accreditation.aun_criteria c WHERE r.id=@Id AND r.criterion_id=c.id AND c.framework_id=@FrameworkId;
        """;
        if(await Execute(sql,new{Id=requirementId,FrameworkId=frameworkId,Code=Required(request.Code,"code"),Order=request.DisplayOrder,Page=request.SourcePage},ct)==0)
            throw new AccreditationNotFoundException("Không tìm thấy requirement.");
    }

    public async Task UpsertCriterionTranslationAsync(long frameworkId,long criterionId,string language,TranslationText request,CancellationToken ct)
    {
        if(language.Equals("en",StringComparison.OrdinalIgnoreCase)) await EnsureDraft(frameworkId,ct);
        const string sql="""
        INSERT INTO accreditation.aun_criterion_translations(criterion_id,language_code,title,description,translation_status,translation_source)
        SELECT c.id,@Language,@Title,@Description,@Status,@Source FROM accreditation.aun_criteria c WHERE c.id=@Id AND c.framework_id=@FrameworkId
        ON CONFLICT(criterion_id,language_code) DO UPDATE SET title=EXCLUDED.title,description=EXCLUDED.description,
        translation_status=EXCLUDED.translation_status,translation_source=EXCLUDED.translation_source,updated_at=now();
        """;
        if(await Execute(sql,new{Id=criterionId,FrameworkId=frameworkId,Language=language.ToLowerInvariant(),Title=Required(request.Title,"title"),request.Description,Status=Clean(request.Status)??"draft",request.Source},ct)==0)
            throw new AccreditationNotFoundException("Không tìm thấy criterion.");
    }

    public async Task UpsertRequirementTranslationAsync(long frameworkId,long requirementId,string language,TranslationText request,CancellationToken ct)
    {
        if(language.Equals("en",StringComparison.OrdinalIgnoreCase)) await EnsureDraft(frameworkId,ct);
        const string sql="""
        INSERT INTO accreditation.aun_requirement_translations(requirement_id,language_code,statement,guidance,translation_status,translation_source)
        SELECT r.id,@Language,@Statement,@Guidance,@Status,@Source FROM accreditation.aun_requirements r
        JOIN accreditation.aun_criteria c ON c.id=r.criterion_id WHERE r.id=@Id AND c.framework_id=@FrameworkId
        ON CONFLICT(requirement_id,language_code) DO UPDATE SET statement=EXCLUDED.statement,guidance=EXCLUDED.guidance,
        translation_status=EXCLUDED.translation_status,translation_source=EXCLUDED.translation_source,updated_at=now();
        """;
        if(await Execute(sql,new{Id=requirementId,FrameworkId=frameworkId,Language=language.ToLowerInvariant(),Statement=Required(request.Statement,"statement"),request.Guidance,Status=Clean(request.Status)??"draft",request.Source},ct)==0)
            throw new AccreditationNotFoundException("Không tìm thấy requirement.");
    }

    public async Task PublishAsync(long id, CancellationToken ct)
    {
        await EnsureDraft(id, ct);
        var tree = await GetTreeAsync(id, "en", ct);
        var request = new FrameworkImportRequest(tree.Code, tree.Version, "programme", tree.DefaultLanguage,
            (await GetFrameworkAsync(id,ct)).SourceTitle, null, tree.Criteria.Select(c => new ImportCriterion(c.Code,c.DisplayOrder,null,c.Requirements.Select(r=>new ImportRequirement(r.Code,r.DisplayOrder,r.SourcePage,null)).ToList())).ToList());
        var result = FrameworkValidator.Validate(request);
        if (!result.Valid) throw new AccreditationValidationException("Framework chưa đủ điều kiện publish.", result.Errors);
        await Execute("UPDATE accreditation.aun_frameworks SET status='published',published_at=now(),updated_at=now() WHERE id=@Id", new { Id=id }, ct);
    }

    public async Task RetireAsync(long id, CancellationToken ct)
    {
        var affected = await Execute("UPDATE accreditation.aun_frameworks SET status='retired',retired_at=now(),updated_at=now() WHERE id=@Id AND status='published'", new { Id=id }, ct);
        if (affected == 0) throw new AccreditationConflictException("invalid_framework_state", "Chỉ framework published mới có thể retire.");
    }

    public async Task DeleteFrameworkAsync(long id, CancellationToken ct)
    {
        var affected = await Execute("DELETE FROM accreditation.aun_frameworks WHERE id=@Id AND status='draft'", new { Id=id }, ct);
        if (affected == 0) throw new AccreditationConflictException("framework_immutable", "Chỉ framework draft mới có thể xóa.");
    }

    public async Task DeleteCriterionAsync(long frameworkId,long criterionId,CancellationToken ct)
    { await EnsureDraft(frameworkId,ct); if(await Execute("DELETE FROM accreditation.aun_criteria WHERE id=@Id AND framework_id=@FrameworkId",new{Id=criterionId,FrameworkId=frameworkId},ct)==0) throw new AccreditationNotFoundException("Không tìm thấy criterion."); }
    public async Task DeleteRequirementAsync(long frameworkId,long requirementId,CancellationToken ct)
    { await EnsureDraft(frameworkId,ct); if(await Execute("DELETE FROM accreditation.aun_requirements r USING accreditation.aun_criteria c WHERE r.id=@Id AND r.criterion_id=c.id AND c.framework_id=@FrameworkId",new{Id=requirementId,FrameworkId=frameworkId},ct)==0) throw new AccreditationNotFoundException("Không tìm thấy requirement."); }

    private async Task EnsureDraft(long id,CancellationToken ct)
    { var status=await ScalarOrNotFound<string>("SELECT status FROM accreditation.aun_frameworks WHERE id=@Id",new{Id=id},ct); if(status!="draft") throw new AccreditationConflictException("framework_immutable","Framework đã publish/retire không thể sửa cấu trúc."); }
    private async Task<long> MutateDraftScalar(long id,string sql,object args,CancellationToken ct)
    { await EnsureDraft(id,ct); await using var c=await dataSource.OpenConnectionAsync(ct); return await c.ExecuteScalarAsync<long>(new CommandDefinition(sql,args,cancellationToken:ct)); }
    private async Task<T> ScalarOrNotFound<T>(string sql,object args,CancellationToken ct)
    { await using var c=await dataSource.OpenConnectionAsync(ct); var value=await c.QuerySingleOrDefaultAsync<T>(new CommandDefinition(sql,args,cancellationToken:ct)); return value is null ? throw new AccreditationNotFoundException("Không tìm thấy dữ liệu.") : value; }
    private async Task<int> Execute(string sql,object args,CancellationToken ct)
    { await using var c=await dataSource.OpenConnectionAsync(ct); return await c.ExecuteAsync(new CommandDefinition(sql,args,cancellationToken:ct)); }
    private static string Required(string? value,string field) => !string.IsNullOrWhiteSpace(value)?value.Trim():throw new AccreditationValidationException("Dữ liệu không hợp lệ.",new Dictionary<string,string[]>{{field,["Trường này là bắt buộc."]}});
    private static string? Clean(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
    private static async Task SaveCriterionTranslations(NpgsqlConnection c,NpgsqlTransaction tx,long id,IReadOnlyDictionary<string,TranslationText>? items,CancellationToken ct)
    { foreach(var x in items??new Dictionary<string,TranslationText>()) await c.ExecuteAsync(new CommandDefinition("INSERT INTO accreditation.aun_criterion_translations(criterion_id,language_code,title,description,translation_status,translation_source) VALUES(@Id,@Language,@Title,@Description,@Status,@Source)",new{Id=id,Language=x.Key,Title=Required(x.Value.Title,"title"),x.Value.Description,Status=Clean(x.Value.Status)??"draft",x.Value.Source},tx,cancellationToken:ct)); }
    private static async Task SaveRequirementTranslations(NpgsqlConnection c,NpgsqlTransaction tx,long id,IReadOnlyDictionary<string,TranslationText>? items,CancellationToken ct)
    { foreach(var x in items??new Dictionary<string,TranslationText>()) await c.ExecuteAsync(new CommandDefinition("INSERT INTO accreditation.aun_requirement_translations(requirement_id,language_code,statement,guidance,translation_status,translation_source) VALUES(@Id,@Language,@Statement,@Guidance,@Status,@Source)",new{Id=id,Language=x.Key,Statement=Required(x.Value.Statement,"statement"),x.Value.Guidance,Status=Clean(x.Value.Status)??"draft",x.Value.Source},tx,cancellationToken:ct)); }
    private sealed record TreeRow(long CriterionId,string CriterionCode,int CriterionOrder,string CriterionTitle,string CriterionLanguage,string CriterionTranslationStatus,long? RequirementId,string? RequirementCode,int? RequirementOrder,int? SourcePage,string? Statement,string? RequirementLanguage,string? RequirementTranslationStatus);
}
