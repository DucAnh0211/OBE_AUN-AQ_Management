namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class ProgramVersion
{
    public long Id { get; set; }

    public long ProgramId { get; set; }

    public string VersionCode { get; set; } = null!;

    public string DatasetStatus { get; set; } = null!;

    public long? SourceVersionId { get; set; }

    public bool IsCurrent { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public virtual ICollection<ImportBatch> ImportBatches { get; set; } = new List<ImportBatch>();

    public virtual ICollection<ProgramVersion> InverseSourceVersion { get; set; } = new List<ProgramVersion>();

    public virtual ICollection<Plo> Plos { get; set; } = new List<Plo>();

    public virtual Program Program { get; set; } = null!;

    public virtual ICollection<ProgramCourse> ProgramCourses { get; set; } = new List<ProgramCourse>();

    public virtual ProgramVersion? SourceVersion { get; set; }
}
