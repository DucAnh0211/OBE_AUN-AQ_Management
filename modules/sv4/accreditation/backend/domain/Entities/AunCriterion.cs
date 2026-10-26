using System;
using System.Collections.Generic;

namespace ObeAunQa.Modules.Accreditation.Domain.Entities;

public partial class AunCriterion
{
    public long Id { get; set; }

    public long FrameworkId { get; set; }

    public string Code { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AunCriterionTranslation> AunCriterionTranslations { get; set; } = new List<AunCriterionTranslation>();

    public virtual ICollection<AunRequirement> AunRequirements { get; set; } = new List<AunRequirement>();

    public virtual AunFramework Framework { get; set; } = null!;
}
