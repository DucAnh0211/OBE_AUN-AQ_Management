using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CloConfiguration : IEntityTypeConfiguration<Clo>
{
    public void Configure(EntityTypeBuilder<Clo> entity)
    {
        entity.HasKey(e => e.Id).HasName("clos_pkey");

        entity.ToTable("clos", "curriculum");

        entity.HasAlternateKey(e => new { e.Id, e.ProgramCourseId }).HasName("clos_id_program_course_id_key");

        entity.HasIndex(e => new { e.ProgramCourseId, e.Code }, "clos_program_course_id_code_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.AssumptionNote).HasColumnName("assumption_note");
        entity.Property(e => e.Code).HasColumnName("code");
        entity.Property(e => e.DatasetStatus)
            .HasDefaultValueSql("'finalized'::text")
            .HasColumnName("dataset_status");
        entity.Property(e => e.LevelCode).HasColumnName("level_code");
        entity.Property(e => e.ProgramCourseId).HasColumnName("program_course_id");
        entity.Property(e => e.Provenance).HasColumnName("provenance");
        entity.Property(e => e.SourceReference)
            .HasColumnType("jsonb")
            .HasColumnName("source_reference");
        entity.Property(e => e.Statement).HasColumnName("statement");

        entity.HasOne(d => d.ProgramCourse).WithMany(p => p.Clos)
            .HasForeignKey(d => d.ProgramCourseId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("clos_program_course_id_fkey");
    }
}
