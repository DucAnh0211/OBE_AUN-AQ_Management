using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class AcademicClassConfiguration : IEntityTypeConfiguration<AcademicClass>
{
    public void Configure(EntityTypeBuilder<AcademicClass> b)
    {
        b.ToTable("academic_classes", "curriculum"); b.HasKey(c => c.Id);
        b.Property(c => c.AcademicPeriod).HasMaxLength(50).IsRequired();
        b.Property(c => c.Code).HasMaxLength(30).IsRequired(); b.Property(c => c.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(c => new { c.ProgramVersionId, c.AcademicPeriod, c.Code }).IsUnique();
        b.HasAlternateKey(c => new { c.Id, c.ProgramVersionId, c.AcademicPeriod });
        b.HasOne<ProgramVersion>().WithMany().HasForeignKey(c => c.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
