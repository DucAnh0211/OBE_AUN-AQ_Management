using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class ChatThreadConfiguration : IEntityTypeConfiguration<ChatThread>
{
    public void Configure(EntityTypeBuilder<ChatThread> b)
    {
        b.ToTable("chat_threads", "knowledge"); b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(120); b.HasIndex(x => new { x.OwnerId, x.UpdatedAt });
    }
}

