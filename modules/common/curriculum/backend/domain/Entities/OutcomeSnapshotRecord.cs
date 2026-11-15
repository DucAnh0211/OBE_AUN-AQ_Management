namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class OutcomeSnapshotRecord
{
    public Guid Id { get; set; }
    public long VersionId { get; set; }
    public string Json { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
}
