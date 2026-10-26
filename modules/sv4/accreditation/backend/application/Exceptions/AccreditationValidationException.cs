namespace ObeAunQa.Modules.Accreditation.Application;

public sealed class AccreditationValidationException(
    string message, IReadOnlyDictionary<string, string[]> errors) : Exception(message)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
