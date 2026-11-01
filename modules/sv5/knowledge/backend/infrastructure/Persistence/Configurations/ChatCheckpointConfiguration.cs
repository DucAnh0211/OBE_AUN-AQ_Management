using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class ChatCheckpointConfiguration : IEntityTypeConfiguration<ChatCheckpoint>
{
    public void Configure(EntityTypeBuilder<ChatCheckpoint> b)
    {
        b.ToTable("chat_checkpoints", "knowledge"); b.HasKey(x => new { x.ExchangeId, x.CheckpointId });
        b.Property(x => x.CheckpointId).HasMaxLength(100); b.Property(x => x.ParentId).HasMaxLength(100);
        b.Property(x => x.Payload).HasColumnType("jsonb");
        b.HasOne<ChatExchange>().WithMany().HasForeignKey(x => x.ExchangeId).OnDelete(DeleteBehavior.Cascade);
    }
}

