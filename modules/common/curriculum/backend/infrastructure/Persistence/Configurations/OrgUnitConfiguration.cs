using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class OrgUnitConfiguration : IEntityTypeConfiguration<OrgUnit>
{
    public void Configure(EntityTypeBuilder<OrgUnit> b)
    {
        b.ToTable("org_units", "curriculum", t => t.HasCheckConstraint("org_unit_kind", "\"Kind\" IN ('faculty', 'department')")); b.HasKey(x => x.Id);
        b.Property(x => x.Kind).HasMaxLength(20).IsRequired();
        b.Property(x => x.Code).HasMaxLength(30).IsRequired(); b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.UniversityId, x.Code }).IsUnique();
        b.HasOne<University>().WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}
