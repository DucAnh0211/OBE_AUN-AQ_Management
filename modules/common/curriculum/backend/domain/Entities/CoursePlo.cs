namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class CoursePlo
{
    public long Id { get; set; }

    public long ProgramVersionId { get; set; }

    public long ProgramCourseId { get; set; }

    public long PloId { get; set; }

    public char WeightCode { get; set; }

    public char ProgressionCode { get; set; }

    public string Provenance { get; set; } = null!;

    public string? Fit { get; set; }

    public string? FitReason { get; set; }

    public virtual ICollection<CloPlo> CloPlos { get; set; } = new List<CloPlo>();

    public virtual Plo Plo { get; set; } = null!;

    public virtual ProgramCourse ProgramCourse { get; set; } = null!;

    public virtual ProgressionCode ProgressionCodeNavigation { get; set; } = null!;

    public virtual WeightCode WeightCodeNavigation { get; set; } = null!;
}
