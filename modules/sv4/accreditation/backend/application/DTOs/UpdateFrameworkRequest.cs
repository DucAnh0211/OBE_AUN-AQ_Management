namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record UpdateFrameworkRequest(
    string? DefaultLanguage, string? SourceTitle, string? SourceUrl);
