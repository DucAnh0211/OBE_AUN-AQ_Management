namespace ObeAunQa.Modules.Knowledge.Domain;
public sealed class ChatCheckpoint
{
    public Guid ExchangeId { get; set; }
    public string CheckpointId { get; set; } = "";
    public string? ParentId { get; set; }
    public string Payload { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

