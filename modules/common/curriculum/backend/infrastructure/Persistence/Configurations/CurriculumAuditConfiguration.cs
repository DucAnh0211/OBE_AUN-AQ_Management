using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CurriculumAuditConfiguration : IEntityTypeConfiguration<CurriculumAuditRecord>
{
    public void Configure(EntityTypeBuilder<CurriculumAuditRecord> e)
    {
        e.ToTable("audit_events", "curriculum"); e.HasKey(x => x.Id);
        e.HasIndex(x => x.CreatedAt); e.Property(x => x.Changes).HasColumnType("jsonb");
    }
}
