namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class OfferingLecturer
{
    public long OfferingId { get; set; }
    public long UserId { get; set; }
    public string Role { get; set; } = "main";
    public DateTime AssignedAt { get; set; }
}
