namespace ObeAunQa.Modules.Reporting.Domain.Entities;

public class Report
{
    public Guid Id { get; set; }

    public long ProgramId { get; set; }

    public long VersionId { get; set; }

    public string Kind { get; set; } = null!;

    public string Title { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string SnapshotHash { get; set; } = null!;

    public string Document { get; set; } = null!;

    public DateTime? RevokedAt { get; set; }

    public string? RevokedBy { get; set; }

    public virtual ICollection<AuditEvent> AuditEvents { get; set; } = new List<AuditEvent>();
}
