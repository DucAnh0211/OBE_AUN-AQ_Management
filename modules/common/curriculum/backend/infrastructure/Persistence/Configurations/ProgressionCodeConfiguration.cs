using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class ProgressionCodeConfiguration : IEntityTypeConfiguration<ProgressionCode>
{
    public void Configure(EntityTypeBuilder<ProgressionCode> entity)
    {
        entity.HasKey(e => e.Code).HasName("progression_codes_pkey");

        entity.ToTable("progression_codes", "curriculum", table => {
            table.HasCheckConstraint("progression_codes_rank_check", "rank > 0");
        });

        entity.Property(e => e.Code)
            .HasMaxLength(1)
            .ValueGeneratedNever()
            .HasColumnName("code");
        entity.HasData(new ProgressionCode { Code = 'I', Rank = 1 }, new ProgressionCode { Code = 'R', Rank = 2 }, new ProgressionCode { Code = 'E', Rank = 3 });
        entity.Property(e => e.Rank).HasColumnName("rank");
    }
}
