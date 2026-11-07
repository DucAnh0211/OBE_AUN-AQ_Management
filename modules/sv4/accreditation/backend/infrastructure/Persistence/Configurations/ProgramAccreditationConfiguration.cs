using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Accreditation.Domain.Entities;

namespace ObeAunQa.Modules.Accreditation.Infrastructure;

internal sealed class ProgramAccreditationConfiguration : IEntityTypeConfiguration<ProgramAccreditation>
{
    public void Configure(EntityTypeBuilder<ProgramAccreditation> entity)
    {
        entity.HasKey(e => e.Id).HasName("program_accreditations_pkey");

        entity.ToTable("program_accreditations", "accreditation", table =>
        {
            table.HasCheckConstraint("program_accreditations_status_check", "status IN ('self_assessed', 'accredited', 'not_met')");
            table.HasCheckConstraint("program_accreditations_rating_check", "verdict_rating IS NULL OR verdict_rating BETWEEN 1 AND 7");
            table.HasCheckConstraint("program_accreditations_valid_check", "valid_until IS NULL OR valid_until >= assessed_on");
        });

        entity.HasIndex(e => new { e.ProgramId, e.FrameworkId, e.AssessedOn }, "program_accreditations_round_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.AssessedOn).HasColumnName("assessed_on");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.CreatedBy).HasColumnName("created_by");
        entity.Property(e => e.FrameworkId).HasColumnName("framework_id");
        entity.Property(e => e.Note).HasColumnName("note");
        entity.Property(e => e.ProgramId).HasColumnName("program_id");
        entity.Property(e => e.Status).HasColumnName("status");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
        entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
        entity.Property(e => e.VerdictRating).HasColumnName("verdict_rating");

        entity.HasOne(d => d.Framework).WithMany()
            .HasForeignKey(d => d.FrameworkId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("program_accreditations_framework_id_fkey");
    }
}
