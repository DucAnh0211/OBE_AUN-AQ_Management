namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class AcademicClass
{
    public long Id { get; set; }
    public long ProgramVersionId { get; set; }
    public string AcademicPeriod { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
