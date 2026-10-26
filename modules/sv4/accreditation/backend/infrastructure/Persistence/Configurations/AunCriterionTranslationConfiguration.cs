using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;
internal sealed class AunCriterionTranslationConfiguration : IEntityTypeConfiguration<AunCriterionTranslation>
{
    public void Configure(EntityTypeBuilder<AunCriterionTranslation> entity)
    {
        entity.HasKey(e => new { e.CriterionId, e.LanguageCode }).HasName("aun_criterion_translations_pkey");

        entity.ToTable("aun_criterion_translations", "accreditation", t => { t.HasCheckConstraint("aun_criterion_translations_translation_status_check", "translation_status IN ('official', 'reviewed', 'draft', 'machine_translated')"); });

        entity.Property(e => e.CriterionId).HasColumnName("criterion_id");
        entity.Property(e => e.LanguageCode).HasColumnName("language_code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.Title).HasColumnName("title");
        entity.Property(e => e.TranslationSource).HasColumnName("translation_source");
        entity.Property(e => e.TranslationStatus)
            .HasDefaultValueSql("'draft'::text")
            .HasColumnName("translation_status");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Criterion).WithMany(p => p.AunCriterionTranslations)
            .HasForeignKey(d => d.CriterionId)
            .HasConstraintName("aun_criterion_translations_criterion_id_fkey");
    }
}
