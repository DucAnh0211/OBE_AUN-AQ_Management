using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public sealed record ReportMetric(string Label, int Numerator, int Denominator, decimal? Percentage);
