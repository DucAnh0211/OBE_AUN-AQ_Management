namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public sealed class GradeChange
{
    public long Id { get; set; }
    public long EnrollmentId { get; set; }
    public long ComponentId { get; set; }
    public decimal? PreviousScore { get; set; }
    public decimal? NewScore { get; set; }
    public long ActorId { get; set; }
    public DateTime ChangedAt { get; set; }
    public string Source { get; set; } = "manual";
}
