using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> entity)
    {
        entity.HasKey(e => e.Id).HasName("programs_pkey");

        entity.ToTable("programs", "curriculum");

        entity.HasIndex(e => new { e.UniversityId, e.Code }, "programs_university_id_code_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.ArchivedAt).HasColumnName("archived_at");
        entity.Property(e => e.Code).HasColumnName("code");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.Name).HasColumnName("name");
        entity.Property(e => e.UniversityId).HasColumnName("university_id");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.University).WithMany(p => p.Programs)
            .HasForeignKey(d => d.UniversityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("programs_university_id_fkey");
    }
}
