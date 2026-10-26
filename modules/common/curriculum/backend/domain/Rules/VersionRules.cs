namespace ObeAunQa.Modules.Curriculum.Domain;

public static class VersionRules
{
    public static bool CanEdit(string status, DateTime? archivedAt) => status == CurriculumStatuses.Draft && archivedAt is null;
    public static bool CanPublish(int activeCourseCount) => activeCourseCount > 0;
}
