using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Application;

/// <summary>Cơ cấu điểm theo đề cương học phần, sổ điểm lớp học phần và quy trình nộp → công bố → phúc khảo.</summary>
public sealed class AcademicService(CurriculumService curriculum, IAcademicRepository repository,
    ICurrentUser user, IAccessControlService access, CourseOfferingService offerings)
{
    private bool IsAdmin => user.Role == SystemRoles.Admin;
    public async Task<IReadOnlyList<AcademicPeriodBatch>> PeriodsAsync(long versionId, long offeringId, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, false, ct);
        return AcademicPeriodCatalog.Build(await repository.PeriodsAsync(offering.ProgramCourseId, ct), DateTime.UtcNow.Year);
    }
    public Task<CourseOfferingInfo> CourseAccessAsync(long versionId, long offeringId, bool write, CancellationToken ct) => offerings.AccessAsync(versionId, offeringId, write, ct);
    public async Task<AssessmentScheme> SchemeAsync(long versionId, long courseId, CancellationToken ct)
    {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer) || !await access.CanReadCourseAsync(courseId, ct)) Denied();
        _ = await curriculum.GetProgramCourseAsync(versionId, courseId, ct);
        return await repository.SchemeAsync(versionId, courseId, ct);
    }
    public async Task<AssessmentScheme> SchemeForOfferingAsync(long versionId, long offeringId, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, false, ct);
        return await repository.SchemeAsync(versionId, offering.ProgramCourseId, ct);
    }
    public async Task<AssessmentScheme> SaveSchemeAsync(long versionId, long courseId, SaveAssessmentScheme request, CancellationToken ct)
    {
        if (!IsAdmin) Denied();
        var course = await curriculum.GetProgramCourseAsync(versionId, courseId, ct);
        var policy = await repository.PolicyForVersionAsync(versionId, ct);
        var current = await repository.SchemeAsync(versionId, courseId, ct);
        // Học phần dưới 2 tín chỉ được phép chỉ có một điểm đánh giá (Thông tư 08/2021, Điều 9).
        AcademicValidator.Scheme(request, course.Credits < 2 ? 1 : policy.MinComponents, policy.MinFinalWeight, current.CloCodes);
        return await repository.SaveSchemeAsync(versionId, courseId, request, ct);
    }
    public async Task<SchemeTemplateResult> ApplyTemplateAsync(long versionId, ApplySchemeTemplate request, CancellationToken ct)
    {
        if (!IsAdmin) Denied();
        _ = await curriculum.GetProgramCourseAsync(versionId, request.SourceCourseId, ct);
        return await repository.ApplyTemplateAsync(versionId, request.SourceCourseId, ct);
    }
    public async Task<GradingPolicyInfo> PolicyAsync(long versionId, CancellationToken ct)
    {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer) || !await access.CanReadVersionAsync(versionId, ct)) Denied();
        _ = await curriculum.GetProgramVersionAsync(versionId, ct);
        return await repository.PolicyForVersionAsync(versionId, ct);
    }
    public async Task<IReadOnlyList<AssessmentCoverageItem>> CoverageAsync(long versionId, CancellationToken ct)
    {
        if (user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer) || !await access.CanReadVersionAsync(versionId, ct)) Denied();
        _ = await curriculum.GetProgramVersionAsync(versionId, ct);
        var rows = await repository.CoverageAsync(versionId, ct);
        if (IsAdmin) return rows;
        var readable = (await access.GetReadableCourseIdsAsync(versionId, ct)).ToHashSet();
        return rows.Where(r => readable.Contains(r.CourseId)).ToArray();
    }
    public async Task<PagedResult<EnrollmentInfo>> StudentsAsync(long versionId, long offeringId, int page, string? search, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, false, ct);
        var query = CurriculumValidator.ListQuery(page, 50, search);
        return await repository.StudentsAsync(offering, query.Page, query.Search, ct);
    }
    public async Task<EnrollmentInfo> EnrollAsync(long versionId, long offeringId, EnrollStudent request, CancellationToken ct)
    {
        var offering = await Writable(versionId, offeringId, ct);
        if (offering.GradeStatus != AcademicCodes.Draft) throw new CurriculumConflictException("grades_not_draft", "Lớp học phần đã nộp hoặc công bố điểm, không thể thêm sinh viên.");
        return await repository.EnrollAsync(offering, AcademicValidator.Student(request with { AcademicPeriod = offering.AcademicPeriod }), ct);
    }
    public async Task<StudentGradebook> GradesAsync(long versionId, long offeringId, long enrollmentId, CancellationToken ct)
    {
        var offering = await CourseAccessAsync(versionId, offeringId, false, ct);
        return await repository.GradesAsync(offering, enrollmentId, ct);
    }
    public async Task<StudentGradebook> SaveGradesAsync(long versionId, long offeringId, long enrollmentId, SaveStudentGrades request, CancellationToken ct)
    {
        var offering = await Writable(versionId, offeringId, ct);
        var published = offering.GradeStatus == AcademicCodes.Published;
        if (offering.GradeStatus != AcademicCodes.Draft && !IsAdmin)
            throw new CurriculumConflictException("grades_locked", "Điểm đã nộp hoặc đã công bố. Liên hệ phòng đào tạo để trả lại hoặc phúc khảo.");
        var normalized = request with { SpecialMark = AcademicValidator.SpecialMark(request.SpecialMark), Reason = AcademicValidator.Reason(request.Reason, published) };
        return await repository.SaveGradesAsync(offering, enrollmentId, normalized, user.UserId, ct);
    }
    public async Task<AcademicImportPreview> ImportAsync(long versionId, long offeringId, AcademicImportRequest request, bool apply, CancellationToken ct)
    {
        var offering = await Writable(versionId, offeringId, ct);
        if (offering.GradeStatus != AcademicCodes.Draft) throw new CurriculumConflictException("grades_not_draft", "Chỉ import khi sổ điểm còn ở trạng thái đang nhập.");
        if (request.OfferingId != offering.Id || request.AcademicPeriod != offering.AcademicPeriod)
            AcademicValidator.Invalid("offeringId", "Bản import không thuộc lớp học phần này. Hãy xem trước lại trong đúng lớp.");
        if (request.Mode is not ("students" or "grades") || request.Rows is null || request.Rows.Count is < 1 or > 500 || request.Rows.Any(r => r is null))
            AcademicValidator.Invalid("rows", "Chọn import sinh viên/điểm và từ 1 đến 500 dòng mỗi lần.");
        return await repository.ImportAsync(offering, request, apply, user.UserId, ct);
    }
    /// <summary>Giảng viên chính nộp điểm; phòng đào tạo (admin) công bố hoặc trả lại kèm lý do.</summary>
    public async Task<CourseOfferingInfo> ChangeGradeStatusAsync(long versionId, long offeringId, string action, GradeStatusChange request, CancellationToken ct)
    {
        var offering = await Writable(versionId, offeringId, ct);
        var isMain = offering.Lecturers.Any(l => l.UserId == user.UserId && l.Role == AcademicCodes.MainLecturer);
        switch (action)
        {
            case "submit" when IsAdmin || isMain: break;
            case "publish" or "reopen" when IsAdmin: break;
            case "submit" or "publish" or "reopen": Denied(); break;
            default: AcademicValidator.Invalid("action", "Thao tác là nộp điểm, công bố hoặc trả lại."); break;
        }
        await repository.ChangeGradeStatusAsync(offering, action, AcademicValidator.Reason(request?.Reason, action == "reopen"), user.UserId, ct);
        return await CourseAccessAsync(versionId, offeringId, false, ct);
    }
    private Task<CourseOfferingInfo> Writable(long versionId, long offeringId, CancellationToken ct) => CourseAccessAsync(versionId, offeringId, true, ct);
    private static void Denied() => throw new AcademicAccessException();
}
