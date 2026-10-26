using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class ProgramAccessRowConfiguration : IEntityTypeConfiguration<ProgramAccessRow>
{
    public void Configure(EntityTypeBuilder<ProgramAccessRow> entity)
    {
        entity.ToTable("programs", "curriculum", t => t.ExcludeFromMigrations());
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
    }
}
