using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class AcademicStudentConfiguration : IEntityTypeConfiguration<AcademicStudent>
{
    public void Configure(EntityTypeBuilder<AcademicStudent> b)
    {
        b.ToTable("academic_students", "curriculum", t => t.HasCheckConstraint("academic_student_status",
            "\"Status\" IN ('studying', 'suspended', 'dropped', 'graduated')")); b.HasKey(x => x.Id);
        b.Property(x => x.StudentCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.CohortCode).HasMaxLength(20);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => new { x.UniversityId, x.StudentCode }).IsUnique();
        b.HasIndex(x => x.UserId).IsUnique();
        b.HasIndex(x => x.HomeClassId);
        b.HasOne<University>().WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<HomeClass>().WithMany().HasForeignKey(x => x.HomeClassId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ProgramVersion>().WithMany().HasForeignKey(x => x.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
