using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CourseOfferingConfiguration : IEntityTypeConfiguration<CourseOffering>
{
    public void Configure(EntityTypeBuilder<CourseOffering> b)
    {
        b.ToTable("course_offerings", "curriculum", t => t.HasCheckConstraint("offering_code_six_digits", "\"Code\" ~ '^[0-9]{6}$'")); b.HasKey(o => o.Id);
        b.Property(o => o.Code).HasMaxLength(6).IsRequired().HasDefaultValueSql("lpad(nextval('curriculum.course_offering_codes')::text, 6, '0')");
        b.Property(o => o.AcademicPeriod).HasMaxLength(50).IsRequired();
        b.HasIndex(o => o.Code).IsUnique(); b.HasIndex(o => new { o.AcademicClassId, o.ProgramCourseId }).IsUnique();
        b.HasIndex(o => o.LecturerId);
        b.HasAlternateKey(o => new { o.Id, o.ProgramCourseId, o.AcademicPeriod });
        b.HasOne(o => o.AcademicClass).WithMany().HasForeignKey(o => new { o.AcademicClassId, o.ProgramVersionId, o.AcademicPeriod })
            .HasPrincipalKey(c => new { c.Id, c.ProgramVersionId, c.AcademicPeriod }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(o => o.ProgramCourse).WithMany().HasForeignKey(o => new { o.ProgramCourseId, o.ProgramVersionId })
            .HasPrincipalKey(c => new { c.Id, c.ProgramVersionId }).OnDelete(DeleteBehavior.Restrict);
    }
}
