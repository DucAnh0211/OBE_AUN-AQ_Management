namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class OutcomePolicyRecord
{
    public long Id { get; set; }
    public long VersionId { get; set; }
    public int Revision { get; set; }
    public string Json { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
}
