namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class Clo
{
    public long Id { get; set; }

    public long ProgramCourseId { get; set; }

    public string Code { get; set; } = null!;

    public string Statement { get; set; } = null!;

    public string? LevelCode { get; set; }

    public string Provenance { get; set; } = null!;

    public string? SourceReference { get; set; }

    public string? AssumptionNote { get; set; }

    public string DatasetStatus { get; set; } = null!;

    public virtual ICollection<CloPlo> CloPlos { get; set; } = new List<CloPlo>();

    public virtual ProgramCourse ProgramCourse { get; set; } = null!;
}
