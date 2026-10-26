using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.SharedKernel;

namespace ObeAunQa.Modules.Curriculum.Application;

public sealed partial class CurriculumService(IProgramRepository programRepository,
    IProgramVersionRepository programVersionRepository,
    IProgramCourseRepository programCourseRepository,
    IPloRepository ploRepository,
    ICloRepository cloRepository,
    ICoursePloMappingRepository coursePloMappingRepository,
    ICloPloMappingRepository cloPloMappingRepository,
    ICurriculumAnalysisRepository curriculumAnalysisRepository,
    IAssignmentCloneService? assignmentCloner = null)
{
    public Task<PagedResult<ProgramResponse>> GetProgramsAsync(
        int page,
        int pageSize,
        string? search,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        var query = CurriculumValidator.ListQuery(page, pageSize, search);
        return programRepository.GetProgramsAsync(
            query.Page,
            query.PageSize,
            query.Search,
            includeArchived,
            cancellationToken);
    }

    public async Task<ProgramResponse> GetProgramAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return await programRepository.GetProgramAsync(id, cancellationToken)
            ?? throw new CurriculumNotFoundException("chương trình đào tạo", id);
    }

    public Task<ProgramResponse> CreateProgramAsync(
        CreateProgramRequest request,
        CancellationToken cancellationToken)
    {
        var program = CurriculumValidator.Program(request);
        return programRepository.CreateProgramAsync(program.Code, program.Name, cancellationToken);
    }

    public async Task<ProgramResponse> UpdateProgramAsync(
        long id,
        UpdateProgramRequest request,
        CancellationToken cancellationToken)
    {
        var name = CurriculumValidator.ProgramName(request);
        return await programRepository.UpdateProgramAsync(id, name, cancellationToken)
            ?? throw new CurriculumNotFoundException("chương trình đào tạo", id);
    }

    public async Task ArchiveProgramAsync(long id, CancellationToken cancellationToken)
    {
        if (!await programRepository.ArchiveProgramAsync(id, cancellationToken))
        {
            throw new CurriculumNotFoundException("chương trình đào tạo", id);
        }
    }

    public async Task<IReadOnlyList<ProgramVersionResponse>> GetProgramVersionsAsync(
        long programId,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramAsync(programId, cancellationToken);
        return await programVersionRepository.GetProgramVersionsAsync(
            programId,
            includeArchived,
            cancellationToken);
    }

    public async Task<ProgramVersionResponse> GetProgramVersionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return await programVersionRepository.GetProgramVersionAsync(id, cancellationToken)
            ?? throw new CurriculumNotFoundException("phiên bản chương trình đào tạo", id);
    }

    public async Task<ProgramVersionResponse> CreateProgramVersionAsync(
        long programId,
        CreateProgramVersionRequest request,
        CancellationToken cancellationToken)
    {
        var versionCode = CurriculumValidator.VersionCode(request.VersionCode);
        var created = await programVersionRepository.CreateProgramVersionAsync(
            programId,
            versionCode,
            request.SourceVersionId,
            cancellationToken);
        if (request.SourceVersionId is not null && assignmentCloner is not null)
            await assignmentCloner.CopyLecturerAssignmentsAsync(
                request.SourceVersionId.Value,created.Id,cancellationToken);
        return created;
    }

    public async Task<ProgramVersionResponse> UpdateProgramVersionAsync(
        long id,
        UpdateProgramVersionRequest request,
        CancellationToken cancellationToken)
    {
        var current = await GetProgramVersionAsync(id, cancellationToken);
        EnsureDraft(current);
        var versionCode = CurriculumValidator.VersionCode(request.VersionCode);

        return await programVersionRepository.UpdateProgramVersionAsync(id, versionCode, cancellationToken)
            ?? throw new CurriculumNotFoundException("phiên bản chương trình đào tạo", id);
    }

    public async Task<ProgramVersionResponse> PublishProgramVersionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var current = await GetProgramVersionAsync(id, cancellationToken);
        EnsureDraft(current);
        return await programVersionRepository.PublishProgramVersionAsync(id, cancellationToken);
    }

    public async Task ArchiveProgramVersionAsync(
        long id,
        CancellationToken cancellationToken)
    {
        if (!await programVersionRepository.ArchiveProgramVersionAsync(id, cancellationToken))
        {
            throw new CurriculumNotFoundException("phiên bản chương trình đào tạo", id);
        }
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
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        var query = CurriculumValidator.ListQuery(page, pageSize, search);
        var normalizedSemester = string.IsNullOrWhiteSpace(semester) ? null : semester.Trim();
        if (normalizedSemester?.Length > 50)
        {
            throw new CurriculumValidationException(new Dictionary<string, string[]>
            {
                ["semester"] = ["Học kỳ không được vượt quá 50 ký tự."]
            });
        }

        return await programCourseRepository.GetProgramCoursesAsync(
            versionId,
            query.Page,
            query.PageSize,
            query.Search,
            normalizedSemester,
            includeArchived,
            cancellationToken);
    }

    public async Task<ProgramCourseResponse> GetProgramCourseAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        _ = await GetProgramVersionAsync(versionId, cancellationToken);
        return await programCourseRepository.GetProgramCourseAsync(
                versionId,
                programCourseId,
                cancellationToken)
            ?? throw new CurriculumNotFoundException("học phần trong phiên bản", programCourseId);
    }

    public async Task<ProgramCourseResponse> CreateProgramCourseAsync(
        long versionId,
        CreateProgramCourseRequest request,
        CancellationToken cancellationToken)
    {
        var version = await GetProgramVersionAsync(versionId, cancellationToken);
        EnsureDraft(version);
        var course = CurriculumValidator.Course(
            request.InstitutionalCode,
            request.Name,
            request.Credits,
            request.Semester,
            request.DisplayOrder,
            requireDisplayOrder: false);

        return await programCourseRepository.CreateProgramCourseAsync(versionId, course, cancellationToken);
    }

    public async Task<ProgramCourseResponse> UpdateProgramCourseAsync(
        long versionId,
        long programCourseId,
        UpdateProgramCourseRequest request,
        CancellationToken cancellationToken)
    {
        var version = await GetProgramVersionAsync(versionId, cancellationToken);
        EnsureDraft(version);
        var course = CurriculumValidator.Course(
            request.InstitutionalCode,
            request.Name,
            request.Credits,
            request.Semester,
            request.DisplayOrder,
            requireDisplayOrder: true);

        return await programCourseRepository.UpdateProgramCourseAsync(
                versionId,
                programCourseId,
                course,
                cancellationToken)
            ?? throw new CurriculumNotFoundException("học phần trong phiên bản", programCourseId);
    }

    public async Task ArchiveProgramCourseAsync(
        long versionId,
        long programCourseId,
        CancellationToken cancellationToken)
    {
        var version = await GetProgramVersionAsync(versionId, cancellationToken);
        EnsureDraft(version);
        if (!await programCourseRepository.ArchiveProgramCourseAsync(
                versionId,
                programCourseId,
                cancellationToken))
        {
            throw new CurriculumNotFoundException("học phần trong phiên bản", programCourseId);
        }
    }

    private static void EnsureDraft(ProgramVersionResponse version)
    {
        if (!VersionRules.CanEdit(version.Status, version.ArchivedAt))
        {
            throw new CurriculumConflictException(
                "version_not_editable",
                "Chỉ phiên bản ở trạng thái draft mới được chỉnh sửa.");
        }
    }
}
