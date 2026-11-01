namespace ObeAunQa.Modules.Knowledge.Domain;
public sealed class ChatExchange
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public string Question { get; set; } = "";
    public string? AnswerJson { get; set; }
    public string Status { get; set; } = "queued";
    public string Phase { get; set; } = "queued";
    public string? Error { get; set; }
    public Guid? Lease { get; set; }
    public DateTime? LeaseUntil { get; set; }
    public int Attempts { get; set; }
    public string? CheckpointId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

