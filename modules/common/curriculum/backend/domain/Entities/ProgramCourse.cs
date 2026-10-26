namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class ProgramCourse
{
    public long Id { get; set; }

    public long ProgramVersionId { get; set; }

    public long CourseId { get; set; }

    public int? SourceRow { get; set; }

    public string? CourseCodeSourceValue { get; set; }

    public int Credits { get; set; }

    public string? SemesterSourceValue { get; set; }

    public string DatasetStatus { get; set; } = null!;

    public string QualityFlags { get; set; } = null!;

    public string? CourseCodeSnapshot { get; set; }

    public string CourseNameSnapshot { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public virtual ICollection<Clo> Clos { get; set; } = new List<Clo>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<CoursePlo> CoursePlos { get; set; } = new List<CoursePlo>();

    public virtual ProgramVersion ProgramVersion { get; set; } = null!;
}
