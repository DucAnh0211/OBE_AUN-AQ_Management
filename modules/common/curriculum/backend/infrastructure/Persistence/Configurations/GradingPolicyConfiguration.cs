using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class GradingPolicyConfiguration : IEntityTypeConfiguration<GradingPolicy>
{
    public void Configure(EntityTypeBuilder<GradingPolicy> b)
    {
        b.ToTable("grading_policies", "curriculum", t => {
            t.HasCheckConstraint("grading_policy_decimals", "\"CourseScoreDecimals\" BETWEEN 0 AND 2 AND \"GpaDecimals\" BETWEEN 1 AND 3");
            t.HasCheckConstraint("grading_policy_components", "\"MinComponents\" BETWEEN 1 AND 10");
            t.HasCheckConstraint("grading_policy_final_weight", "\"MinFinalWeight\" IS NULL OR (\"MinFinalWeight\" > 0 AND \"MinFinalWeight\" <= 100)");
            t.HasCheckConstraint("grading_policy_retake", "\"RetakePolicy\" IN ('highest', 'latest')");
        });
        b.HasKey(x => x.UniversityId); b.Property(x => x.UniversityId).ValueGeneratedNever();
        b.Property(x => x.MinFinalWeight).HasPrecision(5, 2);
        b.Property(x => x.RetakePolicy).HasMaxLength(20).IsRequired();
        b.HasOne<University>().WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Restrict);
    }
}
