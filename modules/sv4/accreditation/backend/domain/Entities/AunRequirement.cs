using System;
using System.Collections.Generic;

namespace ObeAunQa.Modules.Accreditation.Domain.Entities;

public partial class AunRequirement
{
    public long Id { get; set; }

    public long CriterionId { get; set; }

    public string Code { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public int? SourcePage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AunRequirementTranslation> AunRequirementTranslations { get; set; } = new List<AunRequirementTranslation>();

    public virtual AunCriterion Criterion { get; set; } = null!;
}
