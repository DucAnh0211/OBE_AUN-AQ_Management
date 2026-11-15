namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class CurriculumAuditRecord
{
    public long Id { get; set; }
    public long ActorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Entity { get; set; } = "";
    public string Action { get; set; } = "";
    public string Changes { get; set; } = "";
}
