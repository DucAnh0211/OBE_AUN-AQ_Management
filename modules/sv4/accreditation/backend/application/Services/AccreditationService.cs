using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Application;

public sealed class AccreditationService(IAccreditationRepository repository)
{
    public async Task<IReadOnlyList<FrameworkSummary>> GetFrameworksAsync(string? status,string? search,CancellationToken ct) =>
        (await repository.ListAsync(Clean(status),Clean(search),ct)).Select(Summary).ToArray();
    public async Task<FrameworkSummary> GetFrameworkAsync(long id,CancellationToken ct) => Summary(await Find(id,false,ct));
    public async Task<FrameworkTree> GetTreeAsync(long id,string? language,CancellationToken ct) => Tree(await Find(id,false,ct),Clean(language)?.ToLowerInvariant());
    public ImportValidationResult PreviewImport(FrameworkImportRequest request) => FrameworkValidator.Validate(request);

    public async Task<long> CreateFrameworkAsync(CreateFrameworkRequest request,CancellationToken ct)
    {
        var f=new AunFramework {Code=Required(request.Code,"code"),Version=Required(request.Version,"version"),AssessmentLevel=Clean(request.AssessmentLevel)??"programme",DefaultLanguage=Clean(request.DefaultLanguage)??"vi",Status="draft",SourceTitle=Required(request.SourceTitle,"sourceTitle"),SourceUrl=request.SourceUrl};
        await repository.WriteAsync(()=>{repository.Add(f);return Task.FromResult(true);},ct);return f.Id;
    }
    public async Task<long> ImportAsync(FrameworkImportRequest request,CancellationToken ct)
    {
        var validation=FrameworkValidator.Validate(request);
        if(!validation.Valid) throw new AccreditationValidationException("Framework import không hợp lệ.",validation.Errors);
        var f=new AunFramework {Code=request.Code!.Trim(),Version=request.Version!.Trim(),AssessmentLevel=Clean(request.AssessmentLevel)??"programme",DefaultLanguage=Clean(request.DefaultLanguage)??"vi",Status="draft",SourceTitle=request.SourceTitle!,SourceUrl=request.SourceUrl,
            ContentHash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)))).ToLowerInvariant()};
        foreach(var item in request.Criteria!)
        {
            var c=new AunCriterion {Code=item.Code!.Trim(),DisplayOrder=item.DisplayOrder};f.AunCriteria.Add(c);
            foreach(var t in item.Translations??new Dictionary<string,TranslationText>()) SetCriterionTranslation(c,t.Key,t.Value);
            foreach(var outcome in item.Requirements!)
            {
                var r=new AunRequirement {Code=outcome.Code!.Trim(),DisplayOrder=outcome.DisplayOrder,SourcePage=outcome.SourcePage};c.AunRequirements.Add(r);
                foreach(var t in outcome.Translations??new Dictionary<string,TranslationText>()) SetRequirementTranslation(r,t.Key,t.Value);
            }
        }
        await repository.WriteAsync(()=>{repository.Add(f);return Task.FromResult(true);},ct);return f.Id;
    }
    public Task UpdateFrameworkAsync(long id,UpdateFrameworkRequest request,CancellationToken ct) => Mutate(id,true,f=>{
        f.DefaultLanguage=Required(request.DefaultLanguage,"defaultLanguage");f.SourceTitle=Required(request.SourceTitle,"sourceTitle");f.SourceUrl=request.SourceUrl;
    },ct);
    public async Task<long> AddCriterionAsync(long id,CriterionRequest request,CancellationToken ct)
    {
        var c=new AunCriterion {Code=Required(request.Code,"code"),DisplayOrder=request.DisplayOrder};
        await Mutate(id,true,f=>f.AunCriteria.Add(c),ct);return c.Id;
    }
    public Task UpdateCriterionAsync(long id,long criterionId,CriterionRequest request,CancellationToken ct) => Mutate(id,true,f=>{
        var c=Criterion(f,criterionId);c.Code=Required(request.Code,"code");c.DisplayOrder=request.DisplayOrder;c.UpdatedAt=DateTime.UtcNow;
    },ct);
    public async Task<long> AddRequirementAsync(long id,long criterionId,RequirementRequest request,CancellationToken ct)
    {
        var r=new AunRequirement {Code=Required(request.Code,"code"),DisplayOrder=request.DisplayOrder,SourcePage=request.SourcePage};
        await Mutate(id,true,f=>{var c=Criterion(f,criterionId);CheckRequirementCode(c,r.Code);c.AunRequirements.Add(r);},ct);return r.Id;
    }
    public Task UpdateRequirementAsync(long id,long requirementId,RequirementRequest request,CancellationToken ct) => Mutate(id,true,f=>{
        var r=Requirement(f,requirementId);r.Code=Required(request.Code,"code");CheckRequirementCode(r.Criterion,r.Code);r.DisplayOrder=request.DisplayOrder;r.SourcePage=request.SourcePage;r.UpdatedAt=DateTime.UtcNow;
    },ct);
    public Task UpsertCriterionTranslationAsync(long id,long criterionId,string language,TranslationText request,CancellationToken ct) =>
        Mutate(id,language.Equals("en",StringComparison.OrdinalIgnoreCase),f=>SetCriterionTranslation(Criterion(f,criterionId),language.ToLowerInvariant(),request),ct);
    public Task UpsertRequirementTranslationAsync(long id,long requirementId,string language,TranslationText request,CancellationToken ct) =>
        Mutate(id,language.Equals("en",StringComparison.OrdinalIgnoreCase),f=>SetRequirementTranslation(Requirement(f,requirementId),language.ToLowerInvariant(),request),ct);
    public Task PublishAsync(long id,CancellationToken ct) => Mutate(id,true,f=>{
        var request=new FrameworkImportRequest(f.Code,f.Version,f.AssessmentLevel,f.DefaultLanguage,f.SourceTitle,f.SourceUrl,
            f.AunCriteria.Select(c=>new ImportCriterion(c.Code,c.DisplayOrder,null,c.AunRequirements.Select(r=>new ImportRequirement(r.Code,r.DisplayOrder,r.SourcePage,null)).ToList())).ToList());
        var validation=FrameworkValidator.Validate(request);
        if(!validation.Valid) throw new AccreditationValidationException("Framework chưa đủ điều kiện publish.",validation.Errors);
        f.Status="published";f.PublishedAt=DateTime.UtcNow;
    },ct);
    public Task RetireAsync(long id,CancellationToken ct) => Mutate(id,false,f=>{
        if(f.Status!="published") throw new AccreditationConflictException("invalid_framework_state","Chỉ framework published mới có thể retire.");
        f.Status="retired";f.RetiredAt=DateTime.UtcNow;
    },ct);
    public Task DeleteFrameworkAsync(long id,CancellationToken ct) => Mutate(id,true,repository.Remove,ct);
    public Task DeleteCriterionAsync(long id,long criterionId,CancellationToken ct) => Mutate(id,true,f=>f.AunCriteria.Remove(Criterion(f,criterionId)),ct);
    public Task DeleteRequirementAsync(long id,long requirementId,CancellationToken ct) => Mutate(id,true,f=>{var r=Requirement(f,requirementId);r.Criterion.AunRequirements.Remove(r);},ct);

    private Task<bool> Mutate(long id,bool draft,Action<AunFramework> change,CancellationToken ct) => repository.WriteAsync(async ()=>{
        var f=await Find(id,true,ct);
        if(draft && f.Status!="draft") throw new AccreditationConflictException("framework_immutable","Framework đã publish/retire không thể sửa cấu trúc.");
        change(f);f.UpdatedAt=DateTime.UtcNow;return true;
    },ct);
    private async Task<AunFramework> Find(long id,bool tracking,CancellationToken ct) => await repository.FindAsync(id,tracking,ct)??throw new AccreditationNotFoundException("Không tìm thấy framework.");
    private static AunCriterion Criterion(AunFramework f,long id) => f.AunCriteria.SingleOrDefault(c=>c.Id==id)??throw new AccreditationNotFoundException("Không tìm thấy criterion.");
    private static AunRequirement Requirement(AunFramework f,long id) => f.AunCriteria.SelectMany(c=>c.AunRequirements).SingleOrDefault(r=>r.Id==id)??throw new AccreditationNotFoundException("Không tìm thấy requirement.");
    private static void CheckRequirementCode(AunCriterion c,string code) {if(!code.StartsWith(c.Code+".",StringComparison.Ordinal)) throw new AccreditationValidationException("Mã requirement không hợp lệ.",new Dictionary<string,string[]>{{"code",["Mã requirement phải thuộc criterion."]}});}
    private static FrameworkSummary Summary(AunFramework f) => new(f.Id,f.Code,f.Version,f.AssessmentLevel,f.DefaultLanguage,f.Status,f.SourceTitle,f.SourceUrl,f.AunCriteria.Count,f.AunCriteria.Sum(c=>c.AunRequirements.Count),f.PublishedAt,f.RetiredAt);
    private static FrameworkTree Tree(AunFramework f,string? language)
    {
        var requested=language??f.DefaultLanguage;
        var criteria=f.AunCriteria.OrderBy(c=>c.DisplayOrder).Select(c=>{
            var exact=c.AunCriterionTranslations.FirstOrDefault(t=>t.LanguageCode==requested);
            var fallback=c.AunCriterionTranslations.FirstOrDefault(t=>t.LanguageCode==f.DefaultLanguage)??c.AunCriterionTranslations.FirstOrDefault(t=>t.LanguageCode=="en");
            var t=exact??fallback;
            var requirements=c.AunRequirements.OrderBy(r=>r.DisplayOrder).Select(r=>{
                var rx=r.AunRequirementTranslations.FirstOrDefault(t=>t.LanguageCode==requested);
                var rf=r.AunRequirementTranslations.FirstOrDefault(t=>t.LanguageCode==f.DefaultLanguage)??r.AunRequirementTranslations.FirstOrDefault(t=>t.LanguageCode=="en");
                var rt=rx??rf;
                return new RequirementTree(r.Id,r.Code,r.DisplayOrder,r.SourcePage,rt?.Statement??r.Code,rt?.LanguageCode??"none",rx?.TranslationStatus??(rf==null?"missing":"fallback_"+rf.LanguageCode));
            }).ToArray();
            return new CriterionTree(c.Id,c.Code,c.DisplayOrder,t?.Title??c.Code,t?.LanguageCode??"none",exact?.TranslationStatus??(fallback==null?"missing":"fallback_"+fallback.LanguageCode),requirements);
        }).ToArray();
        return new(f.Id,f.Code,f.Version,f.Status,requested,f.DefaultLanguage,criteria.Length,criteria.Sum(c=>c.Requirements.Count),criteria);
    }
    private static void SetCriterionTranslation(AunCriterion c,string language,TranslationText value)
    {
        var t=c.AunCriterionTranslations.SingleOrDefault(t=>t.LanguageCode==language);
        if(t==null){t=new AunCriterionTranslation{LanguageCode=language};c.AunCriterionTranslations.Add(t);}
        t.Title=Required(value.Title,"title");t.Description=value.Description;t.TranslationStatus=Clean(value.Status)??"draft";t.TranslationSource=value.Source;t.UpdatedAt=DateTime.UtcNow;
    }
    private static void SetRequirementTranslation(AunRequirement r,string language,TranslationText value)
    {
        var t=r.AunRequirementTranslations.SingleOrDefault(t=>t.LanguageCode==language);
        if(t==null){t=new AunRequirementTranslation{LanguageCode=language};r.AunRequirementTranslations.Add(t);}
        t.Statement=Required(value.Statement,"statement");t.Guidance=value.Guidance;t.TranslationStatus=Clean(value.Status)??"draft";t.TranslationSource=value.Source;t.UpdatedAt=DateTime.UtcNow;
    }
    private static string? Clean(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
    private static string Required(string? value,string field)=>Clean(value)??throw new AccreditationValidationException("Dữ liệu không hợp lệ.",new Dictionary<string,string[]>{{field,["Trường này là bắt buộc."]}});
}
