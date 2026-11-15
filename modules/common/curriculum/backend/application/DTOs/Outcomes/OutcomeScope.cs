namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeScope(long VersionId, long? OfferingId = null, long? ProgramCourseId = null,
    string? Period = null, string? Cohort = null, bool Preview = false);
