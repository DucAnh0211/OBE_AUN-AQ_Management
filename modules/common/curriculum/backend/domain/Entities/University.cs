namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class University
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public virtual ICollection<Program> Programs { get; set; } = new List<Program>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
