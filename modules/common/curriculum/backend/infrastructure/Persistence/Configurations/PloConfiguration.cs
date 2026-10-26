using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class PloConfiguration : IEntityTypeConfiguration<Plo>
{
    public void Configure(EntityTypeBuilder<Plo> entity)
    {
        entity.HasKey(e => e.Id).HasName("plos_pkey");

        entity.ToTable("plos", "curriculum");

        entity.HasAlternateKey(e => new { e.Id, e.ProgramVersionId }).HasName("plos_id_program_version_id_key");

        entity.HasIndex(e => new { e.ProgramVersionId, e.Code }, "plos_program_version_id_code_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.Code).HasColumnName("code");
        entity.Property(e => e.LevelCode).HasColumnName("level_code");
        entity.Property(e => e.ProgramVersionId).HasColumnName("program_version_id");
        entity.Property(e => e.Provenance)
            .HasDefaultValueSql("'original_curriculum_map'::text")
            .HasColumnName("provenance");
        entity.Property(e => e.SourceCell).HasColumnName("source_cell");
        entity.Property(e => e.Statement).HasColumnName("statement");

        entity.HasOne(d => d.ProgramVersion).WithMany(p => p.Plos)
            .HasForeignKey(d => d.ProgramVersionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("plos_program_version_id_fkey");
    }
}
