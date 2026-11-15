namespace ObeAunQa.SharedKernel;

public sealed class AiServiceException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
