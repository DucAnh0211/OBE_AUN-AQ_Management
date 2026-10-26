namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class Course
{
    public long Id { get; set; }

    public string? InstitutionalCode { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ProgramCourse> ProgramCourses { get; set; } = new List<ProgramCourse>();
}
