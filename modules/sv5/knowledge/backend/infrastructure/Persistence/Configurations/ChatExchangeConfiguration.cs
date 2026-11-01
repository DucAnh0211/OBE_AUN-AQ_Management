using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class ChatExchangeConfiguration : IEntityTypeConfiguration<ChatExchange>
{
    public void Configure(EntityTypeBuilder<ChatExchange> b)
    {
        b.ToTable("chat_exchanges", "knowledge"); b.HasKey(x => x.Id);
        b.Property(x => x.Question).HasMaxLength(2000); b.Property(x => x.AnswerJson).HasColumnType("jsonb");
        b.Property(x => x.Status).HasMaxLength(20); b.Property(x => x.Phase).HasMaxLength(30);
        b.Property(x => x.Error).HasMaxLength(500); b.Property(x => x.CheckpointId).HasMaxLength(100);
        b.HasOne<ChatThread>().WithMany().HasForeignKey(x => x.ThreadId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.ThreadId, x.CreatedAt });
        b.HasIndex(x => x.ThreadId).IsUnique().HasFilter("\"Status\" IN ('queued','processing')");
        b.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}

