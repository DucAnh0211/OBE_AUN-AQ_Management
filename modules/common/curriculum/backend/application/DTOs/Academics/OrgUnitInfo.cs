namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OrgUnitInfo(long Id, long UniversityId, long? ParentId, string Kind, string Code, string Name, bool Archived);
