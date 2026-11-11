namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

/// <summary>Lớp sinh hoạt (lớp hành chính) theo khóa; cố định suốt khóa học, không phụ thuộc học kỳ.</summary>
public sealed class HomeClass
{
    public long Id { get; set; }
    public long ProgramVersionId { get; set; }
    public string? CohortCode { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public long? AdvisorUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
