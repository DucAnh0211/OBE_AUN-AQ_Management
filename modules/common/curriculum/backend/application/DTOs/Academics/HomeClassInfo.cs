namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record HomeClassInfo(long Id, long VersionId, string? CohortCode, string Code, string Name, long? AdvisorUserId, string? AdvisorName, int StudentCount);
