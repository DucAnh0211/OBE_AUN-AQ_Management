namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record ProgramAccreditationResponse(
    long Id,
    long ProgramId,
    long FrameworkId,
    string FrameworkCode,
    string FrameworkVersion,
    string Status,
    string EffectiveStatus,
    DateOnly AssessedOn,
    DateOnly? ValidUntil,
    int? VerdictRating,
    string? Note,
    DateTime CreatedAt,
    DateTime UpdatedAt);
