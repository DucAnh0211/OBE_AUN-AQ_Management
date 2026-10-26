using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Identity.Domain.Entities;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.HasKey(e => e.Id).HasName("refresh_tokens_pkey");

        entity.ToTable("refresh_tokens", "security");

        entity.HasIndex(e => e.TokenHash, "refresh_tokens_token_hash_key").IsUnique();

        entity.HasIndex(e => new { e.UserId, e.ExpiresAt }, "refresh_tokens_user_ix").IsDescending(false, true);

        entity.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.CreatedByIp).HasColumnName("created_by_ip");
        entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        entity.Property(e => e.ReplacedByTokenId).HasColumnName("replaced_by_token_id");
        entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
        entity.Property(e => e.RevokedByIp).HasColumnName("revoked_by_ip");
        entity.Property(e => e.TokenHash)
            .HasMaxLength(64)
            .IsFixedLength()
            .HasColumnName("token_hash");
        entity.Property(e => e.UserId).HasColumnName("user_id");

        entity.HasOne(d => d.ReplacedByToken).WithMany(p => p.InverseReplacedByToken)
            .HasForeignKey(d => d.ReplacedByTokenId)
            .HasConstraintName("refresh_tokens_replaced_by_token_id_fkey");

        entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("refresh_tokens_user_id_fkey");
    }
}
