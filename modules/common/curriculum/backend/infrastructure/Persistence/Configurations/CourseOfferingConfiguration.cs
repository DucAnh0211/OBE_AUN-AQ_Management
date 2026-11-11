using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CourseOfferingConfiguration : IEntityTypeConfiguration<CourseOffering>
{
    public void Configure(EntityTypeBuilder<CourseOffering> b)
    {
        b.ToTable("course_offerings", "curriculum", t => {
            t.HasCheckConstraint("offering_code_six_digits", "\"Code\" ~ '^[0-9]{6}$'");
            t.HasCheckConstraint("offering_grade_status", "\"GradeStatus\" IN ('draft', 'submitted', 'published')");
            t.HasCheckConstraint("offering_capacity_range", "\"Capacity\" IS NULL OR (\"Capacity\" >= 1 AND \"Capacity\" <= 1000)");
        }); b.HasKey(o => o.Id);
        b.Property(o => o.Code).HasMaxLength(6).IsRequired().HasDefaultValueSql("lpad(nextval('curriculum.course_offering_codes')::text, 6, '0')");
        b.Property(o => o.AcademicPeriod).HasMaxLength(50).IsRequired();
        b.Property(o => o.GroupName).HasMaxLength(20).IsRequired();
        b.Property(o => o.GradeStatus).HasMaxLength(20).IsRequired();
        b.Property(o => o.StatusNote).HasMaxLength(500);
        b.HasIndex(o => o.Code).IsUnique();
        b.HasIndex(o => new { o.ProgramCourseId, o.AcademicPeriod, o.GroupName }).IsUnique();
        b.HasIndex(o => new { o.ProgramVersionId, o.AcademicPeriod });
        b.HasAlternateKey(o => new { o.Id, o.ProgramCourseId, o.AcademicPeriod });
        b.HasOne(o => o.ProgramCourse).WithMany().HasForeignKey(o => new { o.ProgramCourseId, o.ProgramVersionId })
            .HasPrincipalKey(c => new { c.Id, c.ProgramVersionId }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<HomeClass>().WithMany().HasForeignKey(o => o.TargetHomeClassId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(o => o.Lecturers).WithOne().HasForeignKey(l => l.OfferingId).OnDelete(DeleteBehavior.Cascade);
    }
}
