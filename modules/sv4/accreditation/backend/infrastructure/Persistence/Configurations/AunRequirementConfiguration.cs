using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;
internal sealed class AunRequirementConfiguration : IEntityTypeConfiguration<AunRequirement>
{
    public void Configure(EntityTypeBuilder<AunRequirement> entity)
    {
        entity.HasKey(e => e.Id).HasName("aun_requirements_pkey");

        entity.ToTable("aun_requirements", "accreditation", t => { t.HasCheckConstraint("aun_requirements_display_order_check", "display_order > 0"); t.HasCheckConstraint("aun_requirements_source_page_check", "source_page IS NULL OR source_page > 0"); });

        entity.HasIndex(e => new { e.CriterionId, e.Code }, "aun_requirements_criterion_id_code_key").IsUnique();

        entity.HasIndex(e => new { e.CriterionId, e.DisplayOrder }, "aun_requirements_criterion_id_display_order_key").IsUnique();

        entity.HasIndex(e => new { e.CriterionId, e.DisplayOrder }, "aun_requirements_criterion_ix");

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.Code).HasColumnName("code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.CriterionId).HasColumnName("criterion_id");
        entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
        entity.Property(e => e.SourcePage).HasColumnName("source_page");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Criterion).WithMany(p => p.AunRequirements)
            .HasForeignKey(d => d.CriterionId)
            .HasConstraintName("aun_requirements_criterion_id_fkey");
    }
}
