namespace ObeAunQa.Modules.Knowledge.Domain;

public sealed class KnowledgeChunk
{
    public Guid DocumentId { get; set; }
    public int Ordinal { get; set; }
    public string Text { get; set; } = "";
    public string Locator { get; set; } = "";
    public int StartOffset { get; set; }
    public float[] Embedding { get; set; } = [];
}
