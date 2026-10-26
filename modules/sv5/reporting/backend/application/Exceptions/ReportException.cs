using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public sealed class ReportException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
