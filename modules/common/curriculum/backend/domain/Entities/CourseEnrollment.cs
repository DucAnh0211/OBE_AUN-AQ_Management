namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class CourseEnrollment
{
    public long Id { get; set; }
    public long ProgramCourseId { get; set; }
    public long OfferingId { get; set; }
    public long StudentId { get; set; }
    public string AcademicPeriod { get; set; } = "";
    public int Revision { get; set; }
    public DateTime CreatedAt { get; set; }
    public AcademicStudent Student { get; set; } = null!;
}
