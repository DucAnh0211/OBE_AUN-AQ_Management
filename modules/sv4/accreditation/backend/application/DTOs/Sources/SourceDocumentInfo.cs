namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record SourceDocumentInfo(Guid Id, string Title, string FileName, string ContentType,
    long Size, string Sha256, long CreatedBy, DateTime CreatedAt, DateTime? DisabledAt);
