namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record SaveProgramAccreditationRequest(
    long? FrameworkId,
    string? Status,
    DateOnly? AssessedOn,
    DateOnly? ValidUntil,
    int? VerdictRating,
    string? Note);
