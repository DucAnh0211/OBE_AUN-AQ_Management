using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class VersionAccessRowConfiguration : IEntityTypeConfiguration<VersionAccessRow>
{
    public void Configure(EntityTypeBuilder<VersionAccessRow> entity)
    {
        entity.ToTable("program_versions", "curriculum", t => t.ExcludeFromMigrations());
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.ProgramId).HasColumnName("program_id");
        entity.Property(x => x.DatasetStatus).HasColumnName("dataset_status");
        entity.Property(x => x.IsCurrent).HasColumnName("is_current");
        entity.Property(x => x.ArchivedAt).HasColumnName("archived_at");
    }
}
