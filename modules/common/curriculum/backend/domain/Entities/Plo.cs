namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class Plo
{
    public long Id { get; set; }

    public long ProgramVersionId { get; set; }

    public string Code { get; set; } = null!;

    public string Statement { get; set; } = null!;

    public string? LevelCode { get; set; }

    public string? SourceCell { get; set; }

    public string Provenance { get; set; } = null!;

    public virtual ICollection<CoursePlo> CoursePlos { get; set; } = new List<CoursePlo>();

    public virtual ProgramVersion ProgramVersion { get; set; } = null!;
}
