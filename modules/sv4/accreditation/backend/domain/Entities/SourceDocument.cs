namespace ObeAunQa.Modules.Accreditation.Domain.Entities;

public sealed class SourceDocument
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
    public string Sha256 { get; set; } = "";
    public string ObjectKey { get; set; } = "";
    public string Bucket { get; set; } = "";
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DisabledAt { get; set; }
    public long? DisabledBy { get; set; }
}
