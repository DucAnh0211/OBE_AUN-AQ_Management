namespace ObeAunQa.Modules.Knowledge.Domain;

public sealed class IngestionJob
{
    public Guid DocumentId { get; set; }
    public string Status { get; set; } = "queued";
    public string Model { get; set; } = "";
    public string PipelineVersion { get; set; } = "python-ocr-v1/1200-150";
    public string? Error { get; set; }
    public string? Warning { get; set; }
    public Guid? Lease { get; set; }
    public DateTime? LeaseUntil { get; set; }
    public bool ForceOcr { get; set; }
    public int Attempts { get; set; }
    public int ChunkCount { get; set; }
    public int Dimensions { get; set; }
    public DateTime UpdatedAt { get; set; }
}
