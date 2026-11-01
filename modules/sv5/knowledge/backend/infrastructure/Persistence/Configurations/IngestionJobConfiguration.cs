using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class IngestionJobConfiguration : IEntityTypeConfiguration<IngestionJob>
{
    public void Configure(EntityTypeBuilder<IngestionJob> b)
    {
        b.ToTable("ingestion_jobs", "knowledge"); b.HasKey(x => x.DocumentId);
        b.Property(x => x.Status).HasMaxLength(20); b.Property(x => x.Model).HasMaxLength(100);
        b.Property(x => x.PipelineVersion).HasMaxLength(100);
        b.Property(x => x.Error).HasMaxLength(500); b.Property(x => x.Warning).HasMaxLength(500);
        b.HasIndex(x => new { x.Status, x.UpdatedAt });
    }
}
