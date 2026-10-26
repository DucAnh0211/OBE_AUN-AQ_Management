using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Identity.Domain.Entities;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class LecturerCourseAssignmentConfiguration : IEntityTypeConfiguration<LecturerCourseAssignment>
{
    public void Configure(EntityTypeBuilder<LecturerCourseAssignment> entity)
    {
        entity.HasOne<CourseAccessRow>().WithMany().HasForeignKey(x => x.ProgramCourseId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("lecturer_course_assignments_program_course_id_fkey");
        entity.HasKey(e => new { e.UserId, e.ProgramCourseId }).HasName("lecturer_course_assignments_pkey");

        entity.ToTable("lecturer_course_assignments", "security");

        entity.HasIndex(e => new { e.ProgramCourseId, e.UserId }, "lecturer_assignments_course_ix");

        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.ProgramCourseId).HasColumnName("program_course_id");
        entity.Property(e => e.AssignedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("assigned_at");
        entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");

        entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.LecturerCourseAssignmentAssignedByNavigations)
            .HasForeignKey(d => d.AssignedBy)
            .HasConstraintName("lecturer_course_assignments_assigned_by_fkey");

        entity.HasOne(d => d.User).WithMany(p => p.LecturerCourseAssignmentUsers)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("lecturer_course_assignments_user_id_fkey");
    }
}
