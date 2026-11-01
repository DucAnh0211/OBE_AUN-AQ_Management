namespace ObeAunQa.Modules.Knowledge.Application;
public sealed class KnowledgeException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
