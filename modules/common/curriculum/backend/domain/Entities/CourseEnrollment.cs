namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class CourseEnrollment
{
    public long Id { get; set; }
    public long ProgramCourseId { get; set; }
    public long OfferingId { get; set; }
    public long StudentId { get; set; }
    public string AcademicPeriod { get; set; } = "";
    public string AttemptKind { get; set; } = "first";
    public string? SpecialMark { get; set; }
    public int Revision { get; set; }
    /// <summary>Kết quả đã công bố; giữ nguyên khi thang điểm thay đổi sau này.</summary>
    public decimal? FinalScore { get; set; }
    public string? LetterGrade { get; set; }
    public decimal? GradePoint { get; set; }
    public bool? Passed { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public AcademicStudent Student { get; set; } = null!;
}
