namespace ObeAunQa.Modules.Reporting.Application;

public interface IReportExporter
{
    byte[] Docx(ReportDocument report);
    byte[] Pdf(ReportDocument report);
}
