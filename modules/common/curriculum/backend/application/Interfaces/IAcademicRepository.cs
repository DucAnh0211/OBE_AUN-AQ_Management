namespace ObeAunQa.Modules.Curriculum.Application;

public interface IAcademicRepository
{
    Task<IReadOnlyList<string>> PeriodsAsync(long courseId, CancellationToken ct);
    Task<GradingPolicyInfo> PolicyForVersionAsync(long versionId, CancellationToken ct);
    Task<AssessmentScheme> SchemeAsync(long versionId, long courseId, CancellationToken ct);
    Task<AssessmentScheme> SaveSchemeAsync(long versionId, long courseId, SaveAssessmentScheme request, CancellationToken ct);
    Task<SchemeTemplateResult> ApplyTemplateAsync(long versionId, long sourceCourseId, CancellationToken ct);
    Task<IReadOnlyList<AssessmentCoverageItem>> CoverageAsync(long versionId, CancellationToken ct);
    Task<PagedResult<EnrollmentInfo>> StudentsAsync(CourseOfferingInfo offering, int page, string? search, CancellationToken ct);
    Task<EnrollmentInfo> EnrollAsync(CourseOfferingInfo offering, EnrollStudent request, CancellationToken ct);
    Task<StudentGradebook> GradesAsync(CourseOfferingInfo offering, long enrollmentId, CancellationToken ct);
    Task<StudentGradebook> SaveGradesAsync(CourseOfferingInfo offering, long enrollmentId, SaveStudentGrades request, long actorId, CancellationToken ct);
    Task<AcademicImportPreview> ImportAsync(CourseOfferingInfo offering, AcademicImportRequest request, bool apply, long actorId, CancellationToken ct);
    /// <summary>action: submit | publish | reopen.</summary>
    Task ChangeGradeStatusAsync(CourseOfferingInfo offering, string action, string? reason, long actorId, CancellationToken ct);
}
