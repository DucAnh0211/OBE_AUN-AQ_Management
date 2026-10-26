using System;
using System.Collections.Generic;

namespace ObeAunQa.Modules.Accreditation.Domain.Entities;

public partial class AunCriterionTranslation
{
    public long CriterionId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string TranslationStatus { get; set; } = null!;

    public string? TranslationSource { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AunCriterion Criterion { get; set; } = null!;
}
