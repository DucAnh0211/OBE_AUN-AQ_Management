using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class AssessmentComponentConfiguration : IEntityTypeConfiguration<AssessmentComponent>
{
    public void Configure(EntityTypeBuilder<AssessmentComponent> b)
    {
        b.ToTable("assessment_components", "curriculum", t => {
            t.HasCheckConstraint("assessment_weight_positive", "\"Weight\" > 0 AND \"Weight\" <= 100");
            t.HasCheckConstraint("assessment_max_score_positive", "\"MaxScore\" > 0 AND \"MaxScore\" <= 1000");
        }); b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(20).IsRequired(); b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Weight).HasPrecision(5, 2); b.Property(x => x.MaxScore).HasPrecision(6, 2);
        b.HasIndex(x => new { x.ProgramVersionId, x.Code }).IsUnique();
        b.HasOne<ProgramVersion>().WithMany().HasForeignKey(x => x.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
