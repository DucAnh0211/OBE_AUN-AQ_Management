using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class ProgramVersionConfiguration : IEntityTypeConfiguration<ProgramVersion>
{
    public void Configure(EntityTypeBuilder<ProgramVersion> entity)
    {
        entity.HasKey(e => e.Id).HasName("program_versions_pkey");

        entity.ToTable("program_versions", "curriculum", table => {
            table.HasCheckConstraint("program_versions_dataset_status_ck", "dataset_status IN ('draft','finalized','archived')");
        });

        entity.HasIndex(e => e.ProgramId, "program_versions_current_uq")
            .IsUnique()
            .HasFilter("(is_current AND (archived_at IS NULL))");

        entity.HasIndex(e => new { e.ProgramId, e.VersionCode }, "program_versions_program_id_version_code_key").IsUnique();

        entity.HasIndex(e => new { e.ProgramId, e.CreatedAt }, "program_versions_program_lookup_ix").IsDescending(false, true);

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.ArchivedAt).HasColumnName("archived_at");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.DatasetStatus)
            .HasDefaultValueSql("'finalized'::text")
            .HasColumnName("dataset_status");
        entity.Property(e => e.IsCurrent).HasDefaultValue(false).HasColumnName("is_current");
        entity.Property(e => e.ProgramId).HasColumnName("program_id");
        entity.Property(e => e.PublishedAt).HasColumnName("published_at");
        entity.Property(e => e.SourceVersionId).HasColumnName("source_version_id");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
        entity.Property(e => e.VersionCode).HasColumnName("version_code");

        entity.HasOne(d => d.Program).WithMany(p => p.ProgramVersions)
            .HasForeignKey(d => d.ProgramId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("program_versions_program_id_fkey");

        entity.HasOne(d => d.SourceVersion).WithMany(p => p.InverseSourceVersion)
            .HasForeignKey(d => d.SourceVersionId)
            .HasConstraintName("program_versions_source_version_id_fkey");
    }
}
