namespace ObeAunQa.Modules.Curriculum.Application;

public interface ICourseOfferingRepository
{
    Task<IReadOnlyList<string>> PeriodsAsync(long versionId, long? lecturerId, CancellationToken ct);
    Task<PagedResult<AcademicClassInfo>> ClassesAsync(long versionId, string period, int page, string? search, long? lecturerId, CancellationToken ct);
    Task<AcademicClassInfo> ClassAsync(long versionId, long classId, long? lecturerId, CancellationToken ct);
    Task<AcademicClassInfo> CreateClassAsync(long versionId, CreateAcademicClass request, CancellationToken ct);
    Task<IReadOnlyList<CourseOfferingInfo>> OfferingsAsync(long versionId, long classId, long? lecturerId, CancellationToken ct);
    Task<PagedResult<CourseOfferingInfo>> SubjectOfferingsAsync(long versionId, long courseId, int page, string? search, long? lecturerId, CancellationToken ct);
    Task<CourseOfferingInfo> OfferingAsync(long versionId, long offeringId, CancellationToken ct);
    Task<CourseOfferingInfo> CreateOfferingAsync(long versionId, long classId, CreateCourseOffering request, CancellationToken ct);
    Task<CourseOfferingInfo> AssignLecturerAsync(long versionId, long offeringId, long? lecturerId, CancellationToken ct);
    Task<long> EnrollmentOfferingAsync(long courseId, long enrollmentId, CancellationToken ct);
}
