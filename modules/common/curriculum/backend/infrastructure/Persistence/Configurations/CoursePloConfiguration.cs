using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CoursePloConfiguration : IEntityTypeConfiguration<CoursePlo>
{
    public void Configure(EntityTypeBuilder<CoursePlo> entity)
    {
        entity.HasKey(e => e.Id).HasName("course_plos_pkey");

        entity.ToTable("course_plos", "curriculum");

        entity.HasAlternateKey(e => new { e.Id, e.ProgramCourseId }).HasName("course_plos_id_program_course_id_key");

        entity.HasIndex(e => new { e.ProgramCourseId, e.PloId }, "course_plos_program_course_id_plo_id_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.Fit).HasColumnName("fit");
        entity.Property(e => e.FitReason).HasColumnName("fit_reason");
        entity.Property(e => e.PloId).HasColumnName("plo_id");
        entity.Property(e => e.ProgramCourseId).HasColumnName("program_course_id");
        entity.Property(e => e.ProgramVersionId).HasColumnName("program_version_id");
        entity.Property(e => e.ProgressionCode)
            .HasMaxLength(1)
            .HasColumnName("progression_code");
        entity.Property(e => e.Provenance).HasColumnName("provenance");
        entity.Property(e => e.WeightCode)
            .HasMaxLength(1)
            .HasColumnName("weight_code");

        entity.HasOne(d => d.ProgressionCodeNavigation).WithMany(p => p.CoursePlos)
            .HasForeignKey(d => d.ProgressionCode)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("course_plos_progression_code_fkey");

        entity.HasOne(d => d.WeightCodeNavigation).WithMany(p => p.CoursePlos)
            .HasForeignKey(d => d.WeightCode)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("course_plos_weight_code_fkey");

        entity.HasOne(d => d.Plo).WithMany(p => p.CoursePlos)
            .HasPrincipalKey(p => new { p.Id, p.ProgramVersionId })
            .HasForeignKey(d => new { d.PloId, d.ProgramVersionId })
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("course_plos_plo_id_program_version_id_fkey");

        entity.HasOne(d => d.ProgramCourse).WithMany(p => p.CoursePlos)
            .HasPrincipalKey(p => new { p.Id, p.ProgramVersionId })
            .HasForeignKey(d => new { d.ProgramCourseId, d.ProgramVersionId })
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("course_plos_program_course_id_program_version_id_fkey");
    }
}
