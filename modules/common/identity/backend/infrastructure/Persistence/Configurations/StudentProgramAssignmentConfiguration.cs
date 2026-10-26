using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Identity.Domain.Entities;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class StudentProgramAssignmentConfiguration : IEntityTypeConfiguration<StudentProgramAssignment>
{
    public void Configure(EntityTypeBuilder<StudentProgramAssignment> entity)
    {
        entity.HasOne<ProgramAccessRow>().WithMany().HasForeignKey(x => x.ProgramId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("student_program_assignments_program_id_fkey");
        entity.HasKey(e => new { e.UserId, e.ProgramId }).HasName("student_program_assignments_pkey");

        entity.ToTable("student_program_assignments", "security");

        entity.HasIndex(e => new { e.ProgramId, e.UserId }, "student_assignments_program_ix");

        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.ProgramId).HasColumnName("program_id");
        entity.Property(e => e.AssignedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("assigned_at");
        entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");

        entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.StudentProgramAssignmentAssignedByNavigations)
            .HasForeignKey(d => d.AssignedBy)
            .HasConstraintName("student_program_assignments_assigned_by_fkey");

        entity.HasOne(d => d.User).WithMany(p => p.StudentProgramAssignmentUsers)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("student_program_assignments_user_id_fkey");
    }
}
