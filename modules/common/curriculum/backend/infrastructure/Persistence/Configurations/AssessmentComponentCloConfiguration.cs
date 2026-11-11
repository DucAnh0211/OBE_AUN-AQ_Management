using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class AssessmentComponentCloConfiguration : IEntityTypeConfiguration<AssessmentComponentClo>
{
    public void Configure(EntityTypeBuilder<AssessmentComponentClo> b)
    {
        b.ToTable("assessment_component_clos", "curriculum", t => t.HasCheckConstraint("assessment_clo_weight", "\"Weight\" > 0 AND \"Weight\" <= 100"));
        b.HasKey(x => new { x.ComponentId, x.CloId }); b.Property(x => x.Weight).HasPrecision(5, 2);
        b.HasIndex(x => x.CloId);
        b.HasOne<AssessmentComponent>().WithMany().HasForeignKey(x => x.ComponentId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Clo>().WithMany().HasForeignKey(x => x.CloId).OnDelete(DeleteBehavior.Cascade);
    }
}
