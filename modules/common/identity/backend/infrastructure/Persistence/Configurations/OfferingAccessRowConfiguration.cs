using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class OfferingAccessRowConfiguration : IEntityTypeConfiguration<OfferingAccessRow>
{
    public void Configure(EntityTypeBuilder<OfferingAccessRow> b)
    {
        b.ToTable("course_offerings", "curriculum", t => t.ExcludeFromMigrations());
        b.HasKey(o => o.Id);
    }
}
