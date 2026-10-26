namespace ObeAunQa.Modules.Accreditation.Application;

public sealed class AccreditationConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
