using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CloPloConfiguration : IEntityTypeConfiguration<CloPlo>
{
    public void Configure(EntityTypeBuilder<CloPlo> entity)
    {
        entity.HasKey(e => new { e.CloId, e.CoursePloId }).HasName("clo_plos_pkey");

        entity.ToTable("clo_plos", "curriculum");

        entity.Property(e => e.CloId).HasColumnName("clo_id");
        entity.Property(e => e.CoursePloId).HasColumnName("course_plo_id");
        entity.Property(e => e.ProgramCourseId).HasColumnName("program_course_id");

        entity.HasOne(d => d.Clo).WithMany(p => p.CloPlos)
            .HasPrincipalKey(p => new { p.Id, p.ProgramCourseId })
            .HasForeignKey(d => new { d.CloId, d.ProgramCourseId })
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("clo_plos_clo_id_program_course_id_fkey");

        entity.HasOne(d => d.CoursePlo).WithMany(p => p.CloPlos)
            .HasPrincipalKey(p => new { p.Id, p.ProgramCourseId })
            .HasForeignKey(d => new { d.CoursePloId, d.ProgramCourseId })
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("clo_plos_course_plo_id_program_course_id_fkey");
    }
}
