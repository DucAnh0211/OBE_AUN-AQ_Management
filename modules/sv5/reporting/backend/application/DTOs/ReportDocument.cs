using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public sealed record ReportDocument(Guid Id, string Kind, string Title, long ProgramId, long VersionId,
    long? ProgramCourseId, string ProgramCode, string ProgramName, string VersionCode, string VersionStatus,
    DateTimeOffset CreatedAt, string CreatedBy, string RuleVersion, string SnapshotHash,
    ReportMetric[] Metrics, string[] Findings, string[] Notes, ReportTable[] Tables,
    CurriculumReportSnapshot Source);
