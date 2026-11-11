namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record StudentInfo(long Id, string StudentCode, string FullName, string? CohortCode, string Status, long? HomeClassId,
    string? HomeClassCode, long? ProgramVersionId, long? UserId);
