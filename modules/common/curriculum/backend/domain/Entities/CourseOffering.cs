namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

/// <summary>Lớp học phần: một học phần trong CTĐT mở ở một học kỳ, có mã 6 số và sổ điểm riêng.</summary>
public sealed class CourseOffering
{
    public long Id { get; set; }
    public long ProgramVersionId { get; set; }
    public long ProgramCourseId { get; set; }
    public string AcademicPeriod { get; set; } = "";
    public string Code { get; set; } = null!;
    public string GroupName { get; set; } = "";
    public int? Capacity { get; set; }
    public long? TargetHomeClassId { get; set; }
    public string GradeStatus { get; set; } = "draft";
    public string? StatusNote { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public long? SubmittedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
    public long? PublishedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProgramCourse ProgramCourse { get; set; } = null!;
    public ICollection<OfferingLecturer> Lecturers { get; set; } = new List<OfferingLecturer>();
}
