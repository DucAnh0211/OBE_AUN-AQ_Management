using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> entity)
    {
        entity.HasKey(e => e.Id).HasName("universities_pkey");

        entity.ToTable("universities", "curriculum");

        entity.HasIndex(e => e.Code, "universities_code_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.ArchivedAt).HasColumnName("archived_at");
        entity.Property(e => e.Code).HasColumnName("code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Name).HasColumnName("name");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");
    }
}
