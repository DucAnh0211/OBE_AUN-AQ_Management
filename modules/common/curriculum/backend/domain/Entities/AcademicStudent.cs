namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class AcademicStudent
{
    public long Id { get; set; }
    public long UniversityId { get; set; }
    public string StudentCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public long? HomeClassId { get; set; }
    public long? ProgramVersionId { get; set; }
    public string? CohortCode { get; set; }
    public string Status { get; set; } = "studying";
    public long? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
