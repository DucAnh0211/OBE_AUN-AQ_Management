namespace ObeAunQa.Modules.Curriculum.Domain.Entities;
public sealed class GradeCorrectionRequest
{
    public Guid Id { get; set; }
    public long OfferingId { get; set; }
    public long EnrollmentId { get; set; }
    public long RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = "pending";
    public string BeforeJson { get; set; } = "{}";
    public string ProposalJson { get; set; } = "{}";
    public long? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? DecisionReason { get; set; }
}
