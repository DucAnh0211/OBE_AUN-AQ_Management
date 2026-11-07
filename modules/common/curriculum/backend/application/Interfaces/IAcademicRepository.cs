namespace ObeAunQa.Modules.Curriculum.Application;

public interface IAcademicRepository
{
    Task<IReadOnlyList<string>> PeriodsAsync(long courseId, CancellationToken ct);
    Task<AssessmentScheme> SchemeAsync(long versionId, CancellationToken ct);
    Task<AssessmentScheme> SaveSchemeAsync(long versionId, SaveAssessmentScheme request, CancellationToken ct);
    Task<PagedResult<EnrollmentInfo>> StudentsAsync(long versionId, long courseId, long offeringId, string period, int page, string? search, CancellationToken ct);
    Task<EnrollmentInfo> EnrollAsync(long versionId, long courseId, long offeringId, EnrollStudent request, CancellationToken ct);
    Task<StudentGradebook> GradesAsync(long courseId, long offeringId, long enrollmentId, CancellationToken ct);
    Task<StudentGradebook> SaveGradesAsync(long versionId, long courseId, long offeringId, long enrollmentId, SaveStudentGrades request, long actorId, CancellationToken ct);
    Task<AcademicImportPreview> ImportAsync(long versionId, long courseId, long offeringId, AcademicImportRequest request, bool apply, long actorId, CancellationToken ct);
}
