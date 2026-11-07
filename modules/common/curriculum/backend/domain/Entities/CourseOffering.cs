namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class CourseOffering
{
    public long Id { get; set; }
    public long AcademicClassId { get; set; }
    public long ProgramVersionId { get; set; }
    public long ProgramCourseId { get; set; }
    public string AcademicPeriod { get; set; } = "";
    public string Code { get; set; } = null!;
    public long? LecturerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public AcademicClass AcademicClass { get; set; } = null!;
    public ProgramCourse ProgramCourse { get; set; } = null!;
}
