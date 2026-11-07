namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class AcademicStudent
{
    public long Id { get; set; }
    public long UniversityId { get; set; }
    public string StudentCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
