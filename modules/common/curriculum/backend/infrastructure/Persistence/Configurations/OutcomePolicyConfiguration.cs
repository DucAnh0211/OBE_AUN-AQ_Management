using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class OutcomePolicyConfiguration : IEntityTypeConfiguration<OutcomePolicyRecord>
{
    public void Configure(EntityTypeBuilder<OutcomePolicyRecord> e)
    {
        e.ToTable("outcome_policies", "curriculum"); e.HasKey(x => x.Id);
        e.HasIndex(x => new { x.VersionId, x.Revision }).IsUnique();
        e.Property(x => x.Json).HasColumnType("jsonb");
        e.HasOne<ProgramVersion>().WithMany().HasForeignKey(x => x.VersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
