namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class CloPlo
{
    public long CloId { get; set; }

    public long ProgramCourseId { get; set; }

    public long CoursePloId { get; set; }

    public virtual Clo Clo { get; set; } = null!;

    public virtual CoursePlo CoursePlo { get; set; } = null!;
}
