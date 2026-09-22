using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;

namespace ObeAunQa.Modules.Curriculum.Tests;

public sealed class CurriculumServiceTests
{
    [Fact]
    public async Task CreateCourse_RejectsFinalizedVersion()
    {
        var repository = new TestRepository
        {
            Version = CreateVersion(CurriculumStatuses.Finalized)
        };
        var service = new CurriculumService(repository);

        var exception = await Assert.ThrowsAsync<CurriculumConflictException>(() =>
            service.CreateProgramCourseAsync(
                1,
                new CreateProgramCourseRequest(null, "Nhập môn lập trình", 3, "Kỳ 1", 1),
                CancellationToken.None));

        Assert.Equal("version_not_editable", exception.Code);
        Assert.Null(repository.ReceivedCourse);
    }

    [Fact]
    public async Task CreateCourse_NormalizesInputForDraftVersion()
    {
        var repository = new TestRepository
        {
            Version = CreateVersion(CurriculumStatuses.Draft)
        };
        var service = new CurriculumService(repository);

        var result = await service.CreateProgramCourseAsync(
            1,
            new CreateProgramCourseRequest(" cs101 ", "  Nhập môn lập trình ", 3, " Kỳ 1 ", null),
            CancellationToken.None);

        Assert.Equal(10, result.Id);
        Assert.NotNull(repository.ReceivedCourse);
        Assert.Equal("CS101", repository.ReceivedCourse.InstitutionalCode);
        Assert.Equal("Kỳ 1", repository.ReceivedCourse.Semester);
    }

    [Fact]
    public async Task UpdateVersion_RejectsFinalizedVersion()
    {
        var repository = new TestRepository
        {
            Version = CreateVersion(CurriculumStatuses.Finalized)
        };
        var service = new CurriculumService(repository);

        await Assert.ThrowsAsync<CurriculumConflictException>(() =>
            service.UpdateProgramVersionAsync(
                1,
                new UpdateProgramVersionRequest("V2"),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreatePlo_RejectsFinalizedVersion()
    {
        var repository = new TestRepository
        {
            Version = CreateVersion(CurriculumStatuses.Finalized)
        };
        var service = new CurriculumService(repository);

        var exception = await Assert.ThrowsAsync<CurriculumConflictException>(() =>
            service.CreatePloAsync(
                1,
                new CreatePloRequest("PLO1", "Vận dụng kiến thức chuyên môn", "C3"),
                CancellationToken.None));

        Assert.Equal("version_not_editable", exception.Code);
        Assert.Null(repository.ReceivedLearningOutcome);
    }

    [Fact]
    public async Task CreatePlo_NormalizesInputForDraftVersion()
    {
        var repository = new TestRepository
        {
            Version = CreateVersion(CurriculumStatuses.Draft)
        };
        var service = new CurriculumService(repository);

        var result = await service.CreatePloAsync(
            1,
            new CreatePloRequest(" plo1 ", "  Vận dụng kiến thức chuyên môn  ", " c3 "),
            CancellationToken.None);

        Assert.Equal(11, result.Id);
        Assert.Equal("PLO1", repository.ReceivedLearningOutcome?.Code);
        Assert.Equal("C3", repository.ReceivedLearningOutcome?.LevelCode);
    }

    private static ProgramVersionResponse CreateVersion(string status)
    {
        return new ProgramVersionResponse(
            1,
            1,
            "V1",
            status,
            status == CurriculumStatuses.Finalized,
            null,
            59,
            status == CurriculumStatuses.Finalized ? DateTime.UtcNow : null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);
    }

    private sealed class TestRepository : ICurriculumRepository
    {
        public required ProgramVersionResponse Version { get; init; }

        public NormalizedCourse? ReceivedCourse { get; private set; }

        public NormalizedLearningOutcome? ReceivedLearningOutcome { get; private set; }

        public Task<ProgramVersionResponse?> GetProgramVersionAsync(
            long id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ProgramVersionResponse?>(Version);
        }

        public Task<ProgramCourseResponse> CreateProgramCourseAsync(
            long versionId,
            NormalizedCourse course,
            CancellationToken cancellationToken)
        {
            ReceivedCourse = course;
            return Task.FromResult(new ProgramCourseResponse(
                10,
                versionId,
                20,
                course.InstitutionalCode,
                course.Name,
                course.Credits,
                course.Semester,
                course.DisplayOrder ?? 1,
                CurriculumStatuses.Draft,
                null,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow));
        }

        public Task<PagedResult<ProgramResponse>> GetProgramsAsync(
            int page,
            int pageSize,
            string? search,
            bool includeArchived,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProgramResponse?> GetProgramAsync(long id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<ProgramResponse> CreateProgramAsync(
            string code,
            string name,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProgramResponse?> UpdateProgramAsync(
            long id,
            string name,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ArchiveProgramAsync(long id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<ProgramVersionResponse>> GetProgramVersionsAsync(
            long programId,
            bool includeArchived,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProgramVersionResponse> CreateProgramVersionAsync(
            long programId,
            string versionCode,
            long? sourceVersionId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProgramVersionResponse?> UpdateProgramVersionAsync(
            long id,
            string versionCode,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProgramVersionResponse> PublishProgramVersionAsync(
            long id,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ArchiveProgramVersionAsync(long id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<PagedResult<ProgramCourseResponse>> GetProgramCoursesAsync(
            long versionId,
            int page,
            int pageSize,
            string? search,
            string? semester,
            bool includeArchived,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProgramCourseResponse?> GetProgramCourseAsync(
            long versionId,
            long programCourseId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ProgramCourseResponse?> UpdateProgramCourseAsync(
            long versionId,
            long programCourseId,
            NormalizedCourse course,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ArchiveProgramCourseAsync(
            long versionId,
            long programCourseId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<PloResponse>> GetPlosAsync(
            long versionId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<PloResponse?> GetPloAsync(
            long versionId,
            long ploId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<PloResponse> CreatePloAsync(
            long versionId,
            NormalizedLearningOutcome plo,
            CancellationToken cancellationToken)
        {
            ReceivedLearningOutcome = plo;
            return Task.FromResult(new PloResponse(
                11,
                versionId,
                plo.Code,
                plo.Statement,
                plo.LevelCode,
                "manual"));
        }

        public Task<PloResponse?> UpdatePloAsync(
            long versionId,
            long ploId,
            NormalizedLearningOutcome plo,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> DeletePloAsync(
            long versionId,
            long ploId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<CloResponse>> GetClosAsync(
            long versionId,
            long programCourseId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<CloResponse?> GetCloAsync(
            long versionId,
            long programCourseId,
            long cloId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<CloResponse> CreateCloAsync(
            long versionId,
            long programCourseId,
            NormalizedLearningOutcome clo,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<CloResponse?> UpdateCloAsync(
            long versionId,
            long programCourseId,
            long cloId,
            NormalizedLearningOutcome clo,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> DeleteCloAsync(
            long versionId,
            long programCourseId,
            long cloId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<CoursePloMappingResponse>> GetCoursePloMappingsAsync(
            long versionId,
            long programCourseId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<CoursePloMappingResponse> CreateCoursePloMappingAsync(
            long versionId,
            long programCourseId,
            NormalizedCoursePloMapping mapping,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<CoursePloMappingResponse?> UpdateCoursePloMappingAsync(
            long versionId,
            long programCourseId,
            long mappingId,
            NormalizedCoursePloMappingUpdate mapping,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> DeleteCoursePloMappingAsync(
            long versionId,
            long programCourseId,
            long mappingId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<CloPloMappingResponse>> GetCloPloMappingsAsync(
            long versionId,
            long programCourseId,
            long cloId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<CloPloMappingResponse> CreateCloPloMappingAsync(
            long versionId,
            long programCourseId,
            long cloId,
            long coursePloId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> DeleteCloPloMappingAsync(
            long versionId,
            long programCourseId,
            long cloId,
            long coursePloId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<CoursePloCreditCheckResponse>> GetPloCreditChecksAsync(
            long versionId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<PloBalanceItemResponse>> GetPloBalanceAsync(
            long versionId,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
