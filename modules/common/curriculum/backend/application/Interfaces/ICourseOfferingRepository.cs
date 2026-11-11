namespace ObeAunQa.Modules.Curriculum.Application;

public interface ICourseOfferingRepository
{
    Task<IReadOnlyList<string>> PeriodsAsync(long versionId, long? lecturerId, CancellationToken ct);
    Task<PagedResult<CourseOfferingInfo>> OfferingsAsync(long versionId, string period, int page, string? search, long? lecturerId, CancellationToken ct);
    Task<PagedResult<CourseOfferingInfo>> SubjectOfferingsAsync(long versionId, long courseId, int page, string? search, long? lecturerId, CancellationToken ct);
    Task<CourseOfferingInfo> OfferingAsync(long versionId, long offeringId, CancellationToken ct);
    Task<CourseOfferingInfo> CreateOfferingAsync(long versionId, CreateCourseOffering request, CancellationToken ct);
    Task<CourseOfferingInfo> UpdateOfferingAsync(long versionId, long offeringId, UpdateCourseOffering request, CancellationToken ct);
    Task<CourseOfferingInfo> SaveLecturersAsync(long versionId, long offeringId, IReadOnlyList<OfferingLecturerInput> lecturers, CancellationToken ct);
    Task<BulkEnrollResult> EnrollHomeClassAsync(long versionId, long offeringId, long homeClassId, CancellationToken ct);
}
