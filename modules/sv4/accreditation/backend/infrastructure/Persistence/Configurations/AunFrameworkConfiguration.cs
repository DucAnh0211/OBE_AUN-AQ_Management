using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;
internal sealed class AunFrameworkConfiguration : IEntityTypeConfiguration<AunFramework>
{
    public void Configure(EntityTypeBuilder<AunFramework> entity)
    {
        entity.HasKey(e => e.Id).HasName("aun_frameworks_pkey");

        entity.ToTable("aun_frameworks", "accreditation", t => { t.HasCheckConstraint("aun_frameworks_status_ck", "status IN ('draft', 'published', 'retired')"); t.HasCheckConstraint("aun_frameworks_level_ck", "assessment_level = 'programme'"); });

        entity.HasIndex(e => new { e.Code, e.Version }, "aun_frameworks_code_version_uq").IsUnique();

        entity.HasIndex(e => new { e.Status, e.CreatedAt }, "aun_frameworks_status_ix").IsDescending(false, true);

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.AssessmentLevel)
            .HasDefaultValueSql("'programme'::text")
            .HasColumnName("assessment_level");
        entity.Property(e => e.Code).HasColumnName("code");
        entity.Property(e => e.ContentHash)
            .HasMaxLength(64)
            .IsFixedLength()
            .HasColumnName("content_hash");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.DefaultLanguage)
            .HasDefaultValueSql("'vi'::text")
            .HasColumnName("default_language");
        entity.Property(e => e.PublishedAt).HasColumnName("published_at");
        entity.Property(e => e.RetiredAt).HasColumnName("retired_at");
        entity.Property(e => e.SourceTitle).HasColumnName("source_title");
        entity.Property(e => e.SourceUrl).HasColumnName("source_url");
        entity.Property(e => e.Status)
            .HasDefaultValueSql("'draft'::text")
            .HasColumnName("status");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
        entity.Property(e => e.Version).HasColumnName("version");
    }
}
