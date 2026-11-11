namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

/// <summary>Quy chế đánh giá của trường; giá trị mặc định theo Thông tư 08/2021/TT-BGDĐT.</summary>
public sealed class GradingPolicy
{
    public long UniversityId { get; set; }
    public int CourseScoreDecimals { get; set; } = 1;
    public int MinComponents { get; set; } = 2;
    public decimal? MinFinalWeight { get; set; }
    public string RetakePolicy { get; set; } = "highest";
    public int GpaDecimals { get; set; } = 2;
    public DateTime UpdatedAt { get; set; }
}
