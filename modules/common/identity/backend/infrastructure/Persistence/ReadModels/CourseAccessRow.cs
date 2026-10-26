namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class CourseAccessRow
{
    public long Id { get; set; }
    public long CourseId { get; set; }
    public long ProgramVersionId { get; set; }
    public DateTime? ArchivedAt { get; set; }
}
