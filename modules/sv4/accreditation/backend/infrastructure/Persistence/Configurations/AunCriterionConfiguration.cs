using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;
internal sealed class AunCriterionConfiguration : IEntityTypeConfiguration<AunCriterion>
{
    public void Configure(EntityTypeBuilder<AunCriterion> entity)
    {
        entity.HasKey(e => e.Id).HasName("aun_criteria_pkey");

        entity.ToTable("aun_criteria", "accreditation", t => { t.HasCheckConstraint("aun_criteria_display_order_check", "display_order > 0"); });

        entity.HasIndex(e => new { e.FrameworkId, e.Code }, "aun_criteria_framework_id_code_key").IsUnique();

        entity.HasIndex(e => new { e.FrameworkId, e.DisplayOrder }, "aun_criteria_framework_id_display_order_key").IsUnique();

        entity.HasIndex(e => new { e.FrameworkId, e.DisplayOrder }, "aun_criteria_framework_ix");

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.Code).HasColumnName("code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
        entity.Property(e => e.FrameworkId).HasColumnName("framework_id");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Framework).WithMany(p => p.AunCriteria)
            .HasForeignKey(d => d.FrameworkId)
            .HasConstraintName("aun_criteria_framework_id_fkey");
    }
}
