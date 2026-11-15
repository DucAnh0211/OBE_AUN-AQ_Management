using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class OrgUnitAssignmentConfiguration : IEntityTypeConfiguration<OrgUnitAssignment>
{
    public void Configure(EntityTypeBuilder<OrgUnitAssignment> e)
    {
        e.ToTable("org_unit_assignments", "curriculum"); e.HasKey(x => new { x.UserId, x.OrgUnitId });
        e.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.OrgUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
