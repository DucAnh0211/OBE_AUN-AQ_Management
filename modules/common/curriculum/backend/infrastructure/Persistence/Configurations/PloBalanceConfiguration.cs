using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class PloBalanceConfiguration : IEntityTypeConfiguration<PloBalance>
{
    public void Configure(EntityTypeBuilder<PloBalance> entity)
    {
        entity
            .HasNoKey()
            .ToView("plo_balance", "curriculum");

        entity.Property(e => e.Deviation).HasColumnName("deviation");
        entity.Property(e => e.Exceeds20Percent).HasColumnName("exceeds_20_percent");
        entity.Property(e => e.MeanScore).HasColumnName("mean_score");
        entity.Property(e => e.PloCode).HasColumnName("plo_code");
        entity.Property(e => e.ProgramVersionId).HasColumnName("program_version_id");
        entity.Property(e => e.Score).HasColumnName("score");
    }
}
