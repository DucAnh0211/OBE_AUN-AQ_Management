namespace ObeAunQa.Modules.Knowledge.Domain;
public sealed class ChatCheckpointWrite
{
    public Guid ExchangeId { get; set; }
    public string CheckpointId { get; set; } = "";
    public string TaskId { get; set; } = "";
    public int Index { get; set; }
    public string Payload { get; set; } = "";
}

