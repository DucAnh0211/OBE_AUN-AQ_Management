using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;
internal sealed class AunRequirementTranslationConfiguration : IEntityTypeConfiguration<AunRequirementTranslation>
{
    public void Configure(EntityTypeBuilder<AunRequirementTranslation> entity)
    {
        entity.HasKey(e => new { e.RequirementId, e.LanguageCode }).HasName("aun_requirement_translations_pkey");

        entity.ToTable("aun_requirement_translations", "accreditation", t => { t.HasCheckConstraint("aun_requirement_translations_translation_status_check", "translation_status IN ('official', 'reviewed', 'draft', 'machine_translated')"); });

        entity.Property(e => e.RequirementId).HasColumnName("requirement_id");
        entity.Property(e => e.LanguageCode).HasColumnName("language_code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Guidance).HasColumnName("guidance");
        entity.Property(e => e.Statement).HasColumnName("statement");
        entity.Property(e => e.TranslationSource).HasColumnName("translation_source");
        entity.Property(e => e.TranslationStatus)
            .HasDefaultValueSql("'draft'::text")
            .HasColumnName("translation_status");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Requirement).WithMany(p => p.AunRequirementTranslations)
            .HasForeignKey(d => d.RequirementId)
            .HasConstraintName("aun_requirement_translations_requirement_id_fkey");
    }
}
