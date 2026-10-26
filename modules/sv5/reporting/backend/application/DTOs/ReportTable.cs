using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public sealed record ReportTable(string Title, string[] Headers, string[][] Rows);
