namespace ObeAunQa.Modules.Identity.Infrastructure;
/// <summary>Read-only view of curriculum.offering_lecturers: which lecturer teaches which course offering.</summary>
internal sealed class OfferingLecturerAccessRow
{
    public long OfferingId { get; set; }
    public long UserId { get; set; }
}
