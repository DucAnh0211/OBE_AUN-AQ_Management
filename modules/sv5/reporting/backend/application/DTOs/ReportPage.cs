using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public sealed record ReportPage(ReportSummary[] Items, int Page, int PageSize, long TotalItems);
