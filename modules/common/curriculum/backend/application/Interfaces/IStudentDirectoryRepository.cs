namespace ObeAunQa.Modules.Curriculum.Application;

public interface IStudentDirectoryRepository
{
    /// <summary>advisorId: chỉ trả về lớp do cố vấn học tập này phụ trách (null = mọi lớp).</summary>
    Task<PagedResult<HomeClassInfo>> HomeClassesAsync(long versionId, int page, string? search, long? advisorId, CancellationToken ct);
    Task<HomeClassInfo> HomeClassAsync(long versionId, long id, CancellationToken ct);
    Task<HomeClassInfo> SaveHomeClassAsync(long versionId, long? id, SaveHomeClass request, CancellationToken ct);
    Task<PagedResult<StudentInfo>> StudentsAsync(long universityId, int page, string? search, long? homeClassId, CancellationToken ct);
    Task<StudentInfo> StudentAsync(long universityId, long id, CancellationToken ct);
    Task<StudentInfo?> StudentByUserAsync(long userId, CancellationToken ct);
    Task<StudentInfo> UpdateStudentAsync(long universityId, long id, UpdateStudent request, CancellationToken ct);
    Task<StudentInfo> AddToHomeClassAsync(long versionId, long homeClassId, AddHomeClassStudent request, CancellationToken ct);
    Task<AcademicImportPreview> ImportHomeClassAsync(long versionId, long homeClassId, IReadOnlyList<AcademicImportRow> rows, bool apply, CancellationToken ct);
    Task<StudentTranscript> TranscriptAsync(long studentId, CancellationToken ct);
    Task<long?> AdvisorOfStudentAsync(long studentId, CancellationToken ct);
}
