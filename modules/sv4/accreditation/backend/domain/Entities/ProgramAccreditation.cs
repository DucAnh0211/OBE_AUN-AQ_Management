namespace ObeAunQa.Modules.Accreditation.Domain.Entities;

// One assessment round of one programme. ProgramId refers to the Curriculum module by value only.
public class ProgramAccreditation
{
    public long Id { get; set; }

    public long ProgramId { get; set; }

    public long FrameworkId { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly AssessedOn { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public int? VerdictRating { get; set; }

    public string? Note { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AunFramework Framework { get; set; } = null!;
}
