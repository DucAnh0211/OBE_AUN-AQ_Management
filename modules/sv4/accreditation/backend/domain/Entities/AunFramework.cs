using System;
using System.Collections.Generic;

namespace ObeAunQa.Modules.Accreditation.Domain.Entities;

public partial class AunFramework
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Version { get; set; } = null!;

    public string AssessmentLevel { get; set; } = null!;

    public string DefaultLanguage { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string SourceTitle { get; set; } = null!;

    public string? SourceUrl { get; set; }

    public string? ContentHash { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? RetiredAt { get; set; }

    public virtual ICollection<AunCriterion> AunCriteria { get; set; } = new List<AunCriterion>();
}
