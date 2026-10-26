using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public sealed record ReportSummary(Guid Id, string Kind, string Title, long ProgramId, long VersionId,
    DateTime CreatedAt, string CreatedBy, string SnapshotHash);
