using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> b)
    {
        b.ToTable("course_enrollments", "curriculum", t => {
            t.HasCheckConstraint("enrollment_attempt_kind", "\"AttemptKind\" IN ('first', 'retake', 'improve')");
            t.HasCheckConstraint("enrollment_special_mark", "\"SpecialMark\" IS NULL OR \"SpecialMark\" IN ('I', 'R', 'P')");
        }); b.HasKey(x => x.Id);
        b.Property(x => x.AcademicPeriod).HasMaxLength(50).IsRequired();
        b.Property(x => x.AttemptKind).HasMaxLength(20).IsRequired();
        b.Property(x => x.SpecialMark).HasMaxLength(2);
        b.Property(x => x.FinalScore).HasPrecision(5, 2); b.Property(x => x.GradePoint).HasPrecision(3, 2);
        b.Property(x => x.LetterGrade).HasMaxLength(5);
        b.HasIndex(x => new { x.OfferingId, x.StudentId }).IsUnique();
        b.HasIndex(x => new { x.StudentId, x.AcademicPeriod });
        b.HasOne<CourseOffering>().WithMany().HasForeignKey(x => new { x.OfferingId, x.ProgramCourseId, x.AcademicPeriod })
            .HasPrincipalKey(o => new { o.Id, o.ProgramCourseId, o.AcademicPeriod }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ProgramCourse>().WithMany().HasForeignKey(x => x.ProgramCourseId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Revision).IsConcurrencyToken();
    }
}
