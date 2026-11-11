using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class OfferingLecturerConfiguration : IEntityTypeConfiguration<OfferingLecturer>
{
    public void Configure(EntityTypeBuilder<OfferingLecturer> b)
    {
        b.ToTable("offering_lecturers", "curriculum", t => t.HasCheckConstraint("offering_lecturer_role", "\"Role\" IN ('main', 'assistant', 'practice')"));
        b.HasKey(x => new { x.OfferingId, x.UserId });
        b.Property(x => x.Role).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.OfferingId).IsUnique().HasFilter("\"Role\" = 'main'").HasDatabaseName("offering_lecturers_one_main");
    }
}
