using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class OfferingLecturerAccessRowConfiguration : IEntityTypeConfiguration<OfferingLecturerAccessRow>
{
    public void Configure(EntityTypeBuilder<OfferingLecturerAccessRow> b)
    {
        b.ToTable("offering_lecturers", "curriculum", t => t.ExcludeFromMigrations());
        b.HasKey(o => new { o.OfferingId, o.UserId });
    }
}
