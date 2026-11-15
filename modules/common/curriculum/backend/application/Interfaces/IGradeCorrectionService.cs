namespace ObeAunQa.Modules.Curriculum.Application;
public interface IGradeCorrectionService
{
    Task<IReadOnlyList<GradeCorrectionInfo>> ListAsync(int page, string? status, CancellationToken ct);
    Task<GradeCorrectionInfo> RequestAsync(long versionId,long offeringId,long enrollmentId,SaveStudentGrades request,CancellationToken ct);
    Task<GradeCorrectionInfo> DecideAsync(Guid id,GradeCorrectionDecision decision,CancellationToken ct);
}
