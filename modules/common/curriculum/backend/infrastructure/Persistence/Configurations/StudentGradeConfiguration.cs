using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class StudentGradeConfiguration : IEntityTypeConfiguration<StudentGrade>
{
    public void Configure(EntityTypeBuilder<StudentGrade> b)
    {
        b.ToTable("student_grades", "curriculum", t => t.HasCheckConstraint("student_grade_nonnegative", "\"Score\" >= 0"));
        b.HasKey(x => new { x.EnrollmentId, x.ComponentId }); b.Property(x => x.Score).HasPrecision(6, 2);
        b.HasOne<CourseEnrollment>().WithMany().HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AssessmentComponent>().WithMany().HasForeignKey(x => x.ComponentId).OnDelete(DeleteBehavior.Restrict);
    }
}
