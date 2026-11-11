namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record UpdateStudent(string FullName, string? CohortCode, string Status, long? HomeClassId, long? UserId);
