using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class WeightCodeConfiguration : IEntityTypeConfiguration<WeightCode>
{
    public void Configure(EntityTypeBuilder<WeightCode> entity)
    {
        entity.HasKey(e => e.Code).HasName("weight_codes_pkey");

        entity.ToTable("weight_codes", "curriculum", table => {
            table.HasCheckConstraint("weight_codes_score_check", "score > 0");
        });

        entity.Property(e => e.Code)
            .HasMaxLength(1)
            .ValueGeneratedNever()
            .HasColumnName("code");
        entity.HasData(new WeightCode { Code = 'X', Score = 2 }, new WeightCode { Code = 'Y', Score = 1 });
        entity.Property(e => e.Score).HasColumnName("score");
    }
}
