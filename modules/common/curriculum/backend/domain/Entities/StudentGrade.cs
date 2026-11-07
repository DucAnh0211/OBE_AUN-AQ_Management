namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class StudentGrade
{
    public long EnrollmentId { get; set; }
    public long ComponentId { get; set; }
    public decimal Score { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long UpdatedBy { get; set; }
}
