using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class ProgramCourseConfiguration : IEntityTypeConfiguration<ProgramCourse>
{
    public void Configure(EntityTypeBuilder<ProgramCourse> entity)
    {
        entity.HasKey(e => e.Id).HasName("program_courses_pkey");

        entity.ToTable("program_courses", "curriculum", table => {
            table.HasCheckConstraint("program_courses_credits_check", "credits > 0");
            table.HasCheckConstraint("program_courses_display_order_ck", "display_order > 0");
            table.HasCheckConstraint("program_courses_dataset_status_ck", "dataset_status IN ('draft','finalized','archived')");
        });

        entity.HasIndex(e => new { e.ProgramVersionId, e.CourseId }, "program_courses_active_course_uq")
            .IsUnique()
            .HasFilter("(archived_at IS NULL)");

        entity.HasAlternateKey(e => new { e.Id, e.ProgramVersionId }).HasName("program_courses_id_program_version_id_key");

        entity.HasIndex(e => new { e.ProgramVersionId, e.SourceRow }, "program_courses_program_version_id_source_row_key").IsUnique();

        entity.HasIndex(e => new { e.ProgramVersionId, e.DisplayOrder, e.Id }, "program_courses_version_lookup_ix");

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.ArchivedAt).HasColumnName("archived_at");
        entity.Property(e => e.CourseCodeSnapshot).HasColumnName("course_code_snapshot");
        entity.Property(e => e.CourseCodeSourceValue).HasColumnName("course_code_source_value");
        entity.Property(e => e.CourseId).HasColumnName("course_id");
        entity.Property(e => e.CourseNameSnapshot).HasColumnName("course_name_snapshot");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Credits).HasColumnName("credits");
        entity.Property(e => e.DatasetStatus)
            .HasDefaultValueSql("'finalized'::text")
            .HasColumnName("dataset_status");
        entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
        entity.Property(e => e.ProgramVersionId).HasColumnName("program_version_id");
        entity.Property(e => e.QualityFlags)
            .HasDefaultValueSql("'[]'::jsonb")
            .HasColumnType("jsonb")
            .HasColumnName("quality_flags");
        entity.Property(e => e.SemesterSourceValue).HasColumnName("semester_source_value");
        entity.Property(e => e.SourceRow).HasColumnName("source_row");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.Course).WithMany(p => p.ProgramCourses)
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("program_courses_course_id_fkey");

        entity.HasOne(d => d.ProgramVersion).WithMany(p => p.ProgramCourses)
            .HasForeignKey(d => d.ProgramVersionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("program_courses_program_version_id_fkey");
    }
}
