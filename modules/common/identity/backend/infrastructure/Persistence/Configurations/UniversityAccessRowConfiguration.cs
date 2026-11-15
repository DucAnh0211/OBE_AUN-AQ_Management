using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class UniversityAccessRowConfiguration : IEntityTypeConfiguration<UniversityAccessRow>
{
    public void Configure(EntityTypeBuilder<UniversityAccessRow> entity) {
        entity.ToTable("universities", "curriculum", t => t.ExcludeFromMigrations());
        entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.ArchivedAt).HasColumnName("archived_at");
    }
}
