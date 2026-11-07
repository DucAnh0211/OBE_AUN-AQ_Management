using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Application;

public sealed class AcademicService(CurriculumService curriculum, IAcademicRepository repository,
    ICurrentUser user, IAccessControlService access, CourseOfferingService offerings)
{
    public async Task<IReadOnlyList<AcademicPeriodBatch>> PeriodsAsync(long versionId, long offeringId, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, false, ct);
        return AcademicPeriodCatalog.Build(await repository.PeriodsAsync(offering.ProgramCourseId, ct), DateTime.UtcNow.Year);
    }
    public Task<CourseOfferingInfo> CourseAccessAsync(long versionId, long offeringId, bool write, CancellationToken ct) => offerings.AccessAsync(versionId, offeringId, write, ct);
    public async Task<AssessmentScheme> SchemeAsync(long versionId, CancellationToken ct)
    {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer) || !await access.CanReadVersionAsync(versionId, ct)) Denied();
        _ = await curriculum.GetProgramVersionAsync(versionId, ct);
        return await repository.SchemeAsync(versionId, ct);
    }
    public async Task<AssessmentScheme> SaveSchemeAsync(long versionId, SaveAssessmentScheme request, CancellationToken ct)
    {
        if (user.Role != SystemRoles.Admin) Denied();
        AcademicValidator.Scheme(request);
        return await repository.SaveSchemeAsync(versionId, request, ct);
    }
    public async Task<PagedResult<EnrollmentInfo>> StudentsAsync(long versionId, long offeringId, string period, int page, string? search, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, false, ct);
        var query = CurriculumValidator.ListQuery(page, 50, search);
        return await repository.StudentsAsync(versionId, offering.ProgramCourseId, offering.Id, offering.AcademicPeriod, query.Page, query.Search, ct);
    }
    public async Task<EnrollmentInfo> EnrollAsync(long versionId, long offeringId, EnrollStudent request, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, true, ct);
        return await repository.EnrollAsync(versionId, offering.ProgramCourseId, offering.Id, AcademicValidator.Student(request with { AcademicPeriod = offering.AcademicPeriod }), ct);
    }
    public async Task<StudentGradebook> GradesAsync(long versionId, long offeringId, long enrollmentId, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, false, ct);
        return await repository.GradesAsync(offering.ProgramCourseId, offering.Id, enrollmentId, ct);
    }
    public async Task<StudentGradebook> SaveGradesAsync(long versionId, long offeringId, long enrollmentId, SaveStudentGrades request, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, true, ct);
        return await repository.SaveGradesAsync(versionId, offering.ProgramCourseId, offering.Id, enrollmentId, request, user.UserId, ct);
    }
    public async Task<AcademicImportPreview> ImportAsync(long versionId, long offeringId, AcademicImportRequest request, bool apply, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, true, ct);
        if (request.OfferingId != offering.Id || request.AcademicPeriod != offering.AcademicPeriod)
            AcademicValidator.Invalid("offeringId", "Bản import không thuộc lớp học phần này. Hãy xem trước lại trong đúng lớp.");
        if (request.Mode is not ("students" or "grades") || request.Rows is null || request.Rows.Count is < 1 or > 500 || request.Rows.Any(r => r is null))
            AcademicValidator.Invalid("rows", "Chọn import sinh viên/điểm và từ 1 đến 500 dòng mỗi lần.");
        return await repository.ImportAsync(versionId, offering.ProgramCourseId, offering.Id, request, apply, user.UserId, ct);
    }
    private static void Denied() => throw new AcademicAccessException();
}
