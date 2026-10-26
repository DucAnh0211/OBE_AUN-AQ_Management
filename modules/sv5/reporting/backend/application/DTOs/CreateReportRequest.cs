using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public sealed record CreateReportRequest(string Kind, long VersionId, long? ProgramCourseId = null);
