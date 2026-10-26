using ObeAunQa.Modules.Curriculum.Domain;

namespace ObeAunQa.Modules.Curriculum.Application;

public sealed partial class CurriculumService
{
    public async Task<IReadOnlyList<PloResponse>> GetPlosAsync(
        long versionId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        return await ploRepository.GetPlosAsync(versionId, cancellationToken);
    }

    public async Task<PloResponse> GetPloAsync(
        long versionId,
        long ploId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        return await ploRepository.GetPloAsync(versionId, ploId, cancellationToken)
            ?? throw new CurriculumNotFoundException("PLO", ploId);
    }

    public async Task<PloResponse> CreatePloAsync(
        long versionId,
        CreatePloRequest request,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        var plo = CurriculumValidator.LearningOutcome(
            request.Code,
            request.Statement,
            request.LevelCode);
        return await ploRepository.CreatePloAsync(versionId, plo, cancellationToken);
    }

    public async Task<PloResponse> UpdatePloAsync(
        long versionId,
        long ploId,
        UpdatePloRequest request,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        var plo = CurriculumValidator.LearningOutcome(
            request.Code,
            request.Statement,
            request.LevelCode);
        return await ploRepository.UpdatePloAsync(versionId, ploId, plo, cancellationToken)
            ?? throw new CurriculumNotFoundException("PLO", ploId);
    }

    public async Task DeletePloAsync(
        long versionId,
        long ploId,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        if (!await ploRepository.DeletePloAsync(versionId, ploId, cancellationToken))
        {
            throw new CurriculumNotFoundException("PLO", ploId);
        }
    }

    public async Task<IReadOnlyList<CloResponse>> GetClosAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramCourseAsync(versionId, programCourseId, cancellationToken);
        return await cloRepository.GetClosAsync(versionId, programCourseId, cancellationToken);
    }

    public async Task<CloResponse> GetCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramCourseAsync(versionId, programCourseId, cancellationToken);
        return await cloRepository.GetCloAsync(versionId, programCourseId, cloId, cancellationToken)
            ?? throw new CurriculumNotFoundException("CLO", cloId);
    }

    public async Task<CloResponse> CreateCloAsync(
        long versionId,
        long programCourseId,
        CreateCloRequest request,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        _ = await GetProgramCourseAsync(versionId, programCourseId, cancellationToken);
        var clo = CurriculumValidator.LearningOutcome(
            request.Code,
            request.Statement,
            request.LevelCode);
        return await cloRepository.CreateCloAsync(
            versionId,
            programCourseId,
            clo,
            cancellationToken);
    }

    public async Task<CloResponse> UpdateCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        UpdateCloRequest request,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        var clo = CurriculumValidator.LearningOutcome(
            request.Code,
            request.Statement,
            request.LevelCode);
        return await cloRepository.UpdateCloAsync(
                versionId,
                programCourseId,
                cloId,
                clo,
                cancellationToken)
            ?? throw new CurriculumNotFoundException("CLO", cloId);
    }

    public async Task DeleteCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        if (!await cloRepository.DeleteCloAsync(
                versionId,
                programCourseId,
                cloId,
                cancellationToken))
        {
            throw new CurriculumNotFoundException("CLO", cloId);
        }
    }

    public async Task<IReadOnlyList<CoursePloMappingResponse>> GetCoursePloMappingsAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramCourseAsync(versionId, programCourseId, cancellationToken);
        return await coursePloMappingRepository.GetCoursePloMappingsAsync(
            versionId,
            programCourseId,
            cancellationToken);
    }

    public async Task<CoursePloMappingResponse> CreateCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        CreateCoursePloMappingRequest request,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        var mapping = CurriculumValidator.CoursePloMapping(request);
        return await coursePloMappingRepository.CreateCoursePloMappingAsync(
            versionId,
            programCourseId,
            mapping,
            cancellationToken);
    }

    public async Task<CoursePloMappingResponse> UpdateCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        UpdateCoursePloMappingRequest request,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        var mapping = CurriculumValidator.CoursePloMapping(request);
        return await coursePloMappingRepository.UpdateCoursePloMappingAsync(
                versionId,
                programCourseId,
                mappingId,
                mapping,
                cancellationToken)
            ?? throw new CurriculumNotFoundException("mapping học phần-PLO", mappingId);
    }

    public async Task DeleteCoursePloMappingAsync(
        long versionId,
        long programCourseId,
        long mappingId,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        if (!await coursePloMappingRepository.DeleteCoursePloMappingAsync(
                versionId,
                programCourseId,
                mappingId,
                cancellationToken))
        {
            throw new CurriculumNotFoundException("mapping học phần-PLO", mappingId);
        }
    }

    public async Task<IReadOnlyList<CloPloMappingResponse>> GetCloPloMappingsAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken)
    {
        _ = await GetCloAsync(versionId, programCourseId, cloId, cancellationToken);
        return await cloPloMappingRepository.GetCloPloMappingsAsync(
            versionId,
            programCourseId,
            cloId,
            cancellationToken);
    }

    public async Task<CloPloMappingResponse> CreateCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CreateCloPloMappingRequest request,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        if (request.CoursePloId <= 0)
        {
            throw new CurriculumValidationException(new Dictionary<string, string[]>
            {
                ["coursePloId"] = ["Mã định danh mapping học phần-PLO phải lớn hơn 0."]
            });
        }

        return await cloPloMappingRepository.CreateCloPloMappingAsync(
            versionId,
            programCourseId,
            cloId,
            request.CoursePloId,
            cancellationToken);
    }

    public async Task DeleteCloPloMappingAsync(
        long versionId,
        long programCourseId,
        long cloId,
        long coursePloId,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        if (!await cloPloMappingRepository.DeleteCloPloMappingAsync(
                versionId,
                programCourseId,
                cloId,
                coursePloId,
                cancellationToken))
        {
            throw new CurriculumNotFoundException("mapping CLO-PLO", coursePloId);
        }
    }

    public async Task<PloCreditCheckResponse> GetPloCreditCheckAsync(
        long versionId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        var courses = await curriculumAnalysisRepository.GetPloCreditChecksAsync(versionId, cancellationToken);
        var violations = courses.Count(course => !course.IsValid);
        return new PloCreditCheckResponse(versionId, CurriculumAnalysisRules.MaximumRequiredPloCount, violations == 0, violations, courses);
    }

    public async Task<PloBalanceResponse> GetPloBalanceAsync(
        long versionId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        var items = await curriculumAnalysisRepository.GetPloBalanceAsync(versionId, cancellationToken);
        return new PloBalanceResponse(
            versionId,
            CurriculumAnalysisRules.AllowedDeviation,
            items.Count > 0 && items.All(item => !item.ExceedsTwentyPercent),
            items);
    }
}
