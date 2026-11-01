using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class ChatCheckpointWriteConfiguration : IEntityTypeConfiguration<ChatCheckpointWrite>
{
    public void Configure(EntityTypeBuilder<ChatCheckpointWrite> b)
    {
        b.ToTable("chat_checkpoint_writes", "knowledge"); b.HasKey(x => new { x.ExchangeId, x.CheckpointId, x.TaskId, x.Index });
        b.Property(x => x.CheckpointId).HasMaxLength(100); b.Property(x => x.TaskId).HasMaxLength(200);
        b.Property(x => x.Payload).HasColumnType("jsonb");
        b.HasOne<ChatCheckpoint>().WithMany().HasForeignKey(x => new { x.ExchangeId, x.CheckpointId }).OnDelete(DeleteBehavior.Cascade);
    }
}

