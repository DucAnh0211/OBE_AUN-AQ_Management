using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class KnowledgeChunkConfiguration : IEntityTypeConfiguration<KnowledgeChunk>
{
    public void Configure(EntityTypeBuilder<KnowledgeChunk> b)
    {
        b.ToTable("chunks", "knowledge"); b.HasKey(x => new { x.DocumentId, x.Ordinal });
        b.Property(x => x.Text).HasMaxLength(1200); b.Property(x => x.Locator).HasMaxLength(100);
        b.Property(x => x.Embedding).HasColumnType("real[]");
        b.HasOne<IngestionJob>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
    }
}
