using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;
internal sealed class GradeCorrectionRequestConfiguration : IEntityTypeConfiguration<GradeCorrectionRequest>
{
    public void Configure(EntityTypeBuilder<GradeCorrectionRequest> e) {
        e.ToTable("grade_correction_requests", "curriculum", t => t.HasCheckConstraint("grade_correction_status_ck", "\"Status\" IN ('pending','approved','rejected')"));
        e.HasKey(x=>x.Id);
        e.Property(x=>x.BeforeJson).HasColumnType("jsonb"); e.Property(x=>x.ProposalJson).HasColumnType("jsonb");
        e.HasIndex(x=>x.EnrollmentId).IsUnique().HasFilter("\"Status\" = 'pending'");
        e.HasOne<CourseOffering>().WithMany().HasForeignKey(x=>x.OfferingId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne<CourseEnrollment>().WithMany().HasForeignKey(x=>x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
