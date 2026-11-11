using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class HomeClassConfiguration : IEntityTypeConfiguration<HomeClass>
{
    public void Configure(EntityTypeBuilder<HomeClass> b)
    {
        b.ToTable("home_classes", "curriculum"); b.HasKey(c => c.Id);
        b.Property(c => c.CohortCode).HasMaxLength(20);
        b.Property(c => c.Code).HasMaxLength(30).IsRequired(); b.Property(c => c.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(c => new { c.ProgramVersionId, c.Code }).IsUnique();
        b.HasIndex(c => c.AdvisorUserId);
        b.HasOne<ProgramVersion>().WithMany().HasForeignKey(c => c.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
