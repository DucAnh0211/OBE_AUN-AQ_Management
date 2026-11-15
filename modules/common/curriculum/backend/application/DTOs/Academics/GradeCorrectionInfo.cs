namespace ObeAunQa.Modules.Curriculum.Application;
public sealed record GradeCorrectionInfo(Guid Id, long VersionId, long OfferingId, string OfferingCode, long EnrollmentId,
    string StudentCode, string StudentName, long RequestedBy, DateTime RequestedAt, string Status,
    IReadOnlyDictionary<string,decimal?> Before, string? PreviousSpecialMark, SaveStudentGrades Proposal,
    long? ReviewedBy, DateTime? ReviewedAt, string? DecisionReason);
