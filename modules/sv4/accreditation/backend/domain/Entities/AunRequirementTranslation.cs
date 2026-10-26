using System;
using System.Collections.Generic;

namespace ObeAunQa.Modules.Accreditation.Domain.Entities;

public partial class AunRequirementTranslation
{
    public long RequirementId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Statement { get; set; } = null!;

    public string? Guidance { get; set; }

    public string TranslationStatus { get; set; } = null!;

    public string? TranslationSource { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AunRequirement Requirement { get; set; } = null!;
}
