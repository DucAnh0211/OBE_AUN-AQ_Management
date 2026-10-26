namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class VersionAccessRow
{
    public long Id { get; set; }
    public long ProgramId { get; set; }
    public string DatasetStatus { get; set; } = null!;
    public bool IsCurrent { get; set; }
    public DateTime? ArchivedAt { get; set; }
}
