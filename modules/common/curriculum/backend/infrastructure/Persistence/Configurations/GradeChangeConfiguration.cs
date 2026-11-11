using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class GradeChangeConfiguration : IEntityTypeConfiguration<GradeChange>
{
    public void Configure(EntityTypeBuilder<GradeChange> b)
    {
        b.ToTable("grade_changes", "curriculum"); b.HasKey(x => x.Id);
        b.Property(x => x.PreviousScore).HasPrecision(6, 2); b.Property(x => x.NewScore).HasPrecision(6, 2);
        b.Property(x => x.Source).HasMaxLength(20).IsRequired(); b.HasIndex(x => new { x.EnrollmentId, x.ChangedAt });
        b.Property(x => x.Reason).HasMaxLength(500);
        b.HasOne<CourseEnrollment>().WithMany().HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AssessmentComponent>().WithMany().HasForeignKey(x => x.ComponentId).OnDelete(DeleteBehavior.Restrict);
    }
}
