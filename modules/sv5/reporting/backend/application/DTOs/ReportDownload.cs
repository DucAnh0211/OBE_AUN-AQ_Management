namespace ObeAunQa.Modules.Reporting.Application;

public sealed record ReportDownload(byte[] Content, string ContentType, string FileName);
