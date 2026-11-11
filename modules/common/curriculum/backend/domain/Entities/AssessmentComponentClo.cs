namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

/// <summary>Đầu điểm đánh giá CLO nào và đóng góp bao nhiêu phần trăm vào CLO đó.</summary>
public sealed class AssessmentComponentClo
{
    public long ComponentId { get; set; }
    public long CloId { get; set; }
    public decimal Weight { get; set; }
}
