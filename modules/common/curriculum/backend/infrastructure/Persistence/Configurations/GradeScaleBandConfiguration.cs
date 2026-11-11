using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class GradeScaleBandConfiguration : IEntityTypeConfiguration<GradeScaleBand>
{
    public void Configure(EntityTypeBuilder<GradeScaleBand> b)
    {
        b.ToTable("grade_scales", "curriculum", t => {
            t.HasCheckConstraint("grade_scale_min_score", "\"MinScore\" >= 0 AND \"MinScore\" <= 10");
            t.HasCheckConstraint("grade_scale_point4", "\"Point4\" >= 0 AND \"Point4\" <= 4");
        }); b.HasKey(x => x.Id);
        b.Property(x => x.Letter).HasMaxLength(5).IsRequired();
        b.Property(x => x.MinScore).HasPrecision(4, 2); b.Property(x => x.Point4).HasPrecision(3, 2);
        b.HasIndex(x => new { x.UniversityId, x.Letter }).IsUnique();
        b.HasOne<University>().WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Restrict);
    }
}
