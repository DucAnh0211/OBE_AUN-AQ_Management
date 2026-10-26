namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class SourceDocument
{
    public long Id { get; set; }

    public string FileName { get; set; } = null!;

    public string FileKind { get; set; } = null!;

    public string Sha256 { get; set; } = null!;

    public string? ObjectKey { get; set; }

    public virtual ICollection<ImportBatch> ImportBatchDatasetDocuments { get; set; } = new List<ImportBatch>();

    public virtual ICollection<ImportBatch> ImportBatchSyllabusDocuments { get; set; } = new List<ImportBatch>();

    public virtual ICollection<ImportBatch> ImportBatchWorkbookDocuments { get; set; } = new List<ImportBatch>();
}
