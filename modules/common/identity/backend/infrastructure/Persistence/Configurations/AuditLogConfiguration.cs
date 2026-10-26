using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Identity.Domain.Entities;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.HasKey(e => e.Id).HasName("audit_logs_pkey");

        entity.ToTable("audit_logs", "security");

        entity.HasIndex(e => new { e.CreatedAt, e.Id }, "audit_logs_created_ix").IsDescending();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.Action).HasColumnName("action");
        entity.Property(e => e.ActorUserId).HasColumnName("actor_user_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Details)
            .HasDefaultValueSql("'{}'::jsonb")
            .HasColumnType("jsonb")
            .HasColumnName("details");
        entity.Property(e => e.IpAddress).HasColumnName("ip_address");
        entity.Property(e => e.TargetId).HasColumnName("target_id");
        entity.Property(e => e.TargetType).HasColumnName("target_type");

        entity.HasOne(d => d.ActorUser).WithMany(p => p.AuditLogs)
            .HasForeignKey(d => d.ActorUserId)
            .HasConstraintName("audit_logs_actor_user_id_fkey");
    }
}
