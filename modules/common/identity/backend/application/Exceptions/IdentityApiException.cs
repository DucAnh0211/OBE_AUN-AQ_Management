namespace ObeAunQa.Modules.Identity;

public sealed class IdentityApiException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}
