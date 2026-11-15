using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class OutcomeSnapshotConfiguration : IEntityTypeConfiguration<OutcomeSnapshotRecord>
{
    public void Configure(EntityTypeBuilder<OutcomeSnapshotRecord> e)
    {
        e.ToTable("outcome_snapshots", "curriculum"); e.HasKey(x => x.Id);
        e.HasIndex(x => new { x.VersionId, x.CreatedAt }); e.Property(x => x.Json).HasColumnType("jsonb");
        e.HasOne<ProgramVersion>().WithMany().HasForeignKey(x => x.VersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
