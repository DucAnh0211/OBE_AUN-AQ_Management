using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Reporting.Domain.Entities;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

internal sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> entity)
    {
        entity.HasKey(e => e.Id).HasName("reports_pkey");

        entity.ToTable("reports", "reporting", table => {
            table.HasCheckConstraint("reports_kind_check", "kind IN ('course','clo','plo')");
        });

        entity.HasIndex(e => new { e.ProgramId, e.CreatedAt, e.Id }, "reports_program_created_idx").IsDescending(false, true, false);

        entity.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.CreatedBy).HasColumnName("created_by");
        entity.Property(e => e.Document)
            .HasColumnType("jsonb")
            .HasColumnName("document");
        entity.Property(e => e.Kind).HasColumnName("kind");
        entity.Property(e => e.ProgramId).HasColumnName("program_id");
        entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
        entity.Property(e => e.RevokedBy).HasColumnName("revoked_by");
        entity.Property(e => e.SnapshotHash)
            .HasMaxLength(64)
            .IsFixedLength()
            .HasColumnName("snapshot_hash");
        entity.Property(e => e.Title).HasColumnName("title");
        entity.Property(e => e.VersionId).HasColumnName("version_id");
    }
}
