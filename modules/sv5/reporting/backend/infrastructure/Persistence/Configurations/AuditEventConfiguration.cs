using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Reporting.Domain.Entities;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

internal sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> entity)
    {
        entity.HasKey(e => e.Id).HasName("audit_events_pkey");

        entity.ToTable("audit_events", "reporting", table => {
            table.HasCheckConstraint("audit_events_action_check", "action IN ('created','viewed','download-docx','download-pdf','revoked')");
        });

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.Action).HasColumnName("action");
        entity.Property(e => e.ActorId).HasColumnName("actor_id");
        entity.Property(e => e.OccurredAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("occurred_at");
        entity.Property(e => e.ReportId).HasColumnName("report_id");

        entity.HasOne(d => d.Report).WithMany(p => p.AuditEvents)
            .HasForeignKey(d => d.ReportId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("audit_events_report_id_fkey");
    }
}
