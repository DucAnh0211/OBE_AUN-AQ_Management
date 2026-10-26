namespace ObeAunQa.Modules.Reporting.Domain.Entities;

public class AuditEvent
{
    public long Id { get; set; }

    public Guid ReportId { get; set; }

    public string ActorId { get; set; } = null!;

    public string Action { get; set; } = null!;

    public DateTime OccurredAt { get; set; }

    public virtual Report Report { get; set; } = null!;
}
