namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class AssessmentComponent
{
    public long Id { get; set; }
    public long ProgramVersionId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Weight { get; set; }
    public decimal MaxScore { get; set; }
    public int DisplayOrder { get; set; }
}
