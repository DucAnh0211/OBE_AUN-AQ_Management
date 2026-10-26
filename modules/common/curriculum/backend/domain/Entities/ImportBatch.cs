namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class ImportBatch
{
    public long Id { get; set; }

    public long ProgramVersionId { get; set; }

    public long DatasetDocumentId { get; set; }

    public long WorkbookDocumentId { get; set; }

    public long SyllabusDocumentId { get; set; }

    public string DatasetSha256 { get; set; } = null!;

    public DateTime ImportedAt { get; set; }

    public virtual SourceDocument DatasetDocument { get; set; } = null!;

    public virtual ProgramVersion ProgramVersion { get; set; } = null!;

    public virtual ICollection<SourceCourseRow> SourceCourseRows { get; set; } = new List<SourceCourseRow>();

    public virtual SourceDocument SyllabusDocument { get; set; } = null!;

    public virtual SourceDocument WorkbookDocument { get; set; } = null!;
}
