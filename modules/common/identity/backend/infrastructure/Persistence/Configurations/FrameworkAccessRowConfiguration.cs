using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class FrameworkAccessRowConfiguration : IEntityTypeConfiguration<FrameworkAccessRow>
{
    public void Configure(EntityTypeBuilder<FrameworkAccessRow> entity)
    {
        entity.ToTable("aun_frameworks", "accreditation", t => t.ExcludeFromMigrations());
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Status).HasColumnName("status");
    }
}
