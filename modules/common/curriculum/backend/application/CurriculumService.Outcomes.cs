namespace ObeAunQa.Modules.Curriculum.Application;

public sealed partial class CurriculumService
{
    public async Task<IReadOnlyList<PloResponse>> GetPlosAsync(
        long versionId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        return await repository.GetPlosAsync(versionId, cancellationToken);
    }

    public async Task<PloResponse> GetPloAsync(
        long versionId,
        long ploId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        return await repository.GetPloAsync(versionId, ploId, cancellationToken)
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
        return await repository.CreatePloAsync(versionId, plo, cancellationToken);
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
        return await repository.UpdatePloAsync(versionId, ploId, plo, cancellationToken)
            ?? throw new CurriculumNotFoundException("PLO", ploId);
    }

    public async Task DeletePloAsync(
        long versionId,
        long ploId,
        CancellationToken cancellationToken)
    {
        EnsureDraft(await GetProgramVersionAsync(versionId, cancellationToken));
        if (!await repository.DeletePloAsync(versionId, ploId, cancellationToken))
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
        return await repository.GetClosAsync(versionId, programCourseId, cancellationToken);
    }

    public async Task<CloResponse> GetCloAsync(
        long versionId,
        long programCourseId,
        long cloId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramCourseAsync(versionId, programCourseId, cancellationToken);
        return await repository.GetCloAsync(versionId, programCourseId, cloId, cancellationToken)
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
        return await repository.CreateCloAsync(
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
        return await repository.UpdateCloAsync(
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
        if (!await repository.DeleteCloAsync(
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
        return await repository.GetCoursePloMappingsAsync(
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
        return await repository.CreateCoursePloMappingAsync(
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
        return await repository.UpdateCoursePloMappingAsync(
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
        if (!await repository.DeleteCoursePloMappingAsync(
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
        return await repository.GetCloPloMappingsAsync(
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

        return await repository.CreateCloPloMappingAsync(
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
        if (!await repository.DeleteCloPloMappingAsync(
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
        var courses = await repository.GetPloCreditChecksAsync(versionId, cancellationToken);
        var violations = courses.Count(course => !course.IsValid);
        return new PloCreditCheckResponse(versionId, 5, violations == 0, violations, courses);
    }

    public async Task<PloBalanceResponse> GetPloBalanceAsync(
        long versionId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        var items = await repository.GetPloBalanceAsync(versionId, cancellationToken);
        return new PloBalanceResponse(
            versionId,
            0.20m,
            items.Count > 0 && items.All(item => !item.ExceedsTwentyPercent),
            items);
    }
}
