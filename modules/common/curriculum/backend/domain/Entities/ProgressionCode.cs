namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class ProgressionCode
{
    public char Code { get; set; }

    public int Rank { get; set; }

    public virtual ICollection<CoursePlo> CoursePlos { get; set; } = new List<CoursePlo>();
}
