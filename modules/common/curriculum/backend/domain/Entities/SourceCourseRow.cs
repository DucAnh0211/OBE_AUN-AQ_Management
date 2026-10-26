namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class SourceCourseRow
{
    public long ImportBatchId { get; set; }

    public int SourceRow { get; set; }

    public string RawData { get; set; } = null!;

    public virtual ImportBatch ImportBatch { get; set; } = null!;
}
