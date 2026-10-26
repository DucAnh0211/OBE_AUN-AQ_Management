namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class Program
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public virtual ICollection<ProgramVersion> ProgramVersions { get; set; } = new List<ProgramVersion>();
}
