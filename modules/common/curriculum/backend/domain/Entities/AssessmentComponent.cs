namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

/// <summary>Đầu điểm trong đề cương của một học phần.</summary>
public sealed class AssessmentComponent
{
    public long Id { get; set; }
    public long ProgramCourseId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "process";
    public decimal Weight { get; set; }
    public decimal MaxScore { get; set; }
    public int DisplayOrder { get; set; }
}
