namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class GradeScaleBand
{
    public long Id { get; set; }
    public long UniversityId { get; set; }
    public string Letter { get; set; } = "";
    public decimal MinScore { get; set; }
    public decimal Point4 { get; set; }
    public bool IsPassing { get; set; }
    public int DisplayOrder { get; set; }
}
