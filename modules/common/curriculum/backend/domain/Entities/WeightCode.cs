namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class WeightCode
{
    public char Code { get; set; }

    public int Score { get; set; }

    public virtual ICollection<CoursePlo> CoursePlos { get; set; } = new List<CoursePlo>();
}
