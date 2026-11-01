namespace ObeAunQa.Modules.Accreditation.Application;

public sealed class DocumentException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
