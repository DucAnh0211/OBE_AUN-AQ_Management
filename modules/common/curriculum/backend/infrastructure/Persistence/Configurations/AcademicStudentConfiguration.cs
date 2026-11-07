using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class AcademicStudentConfiguration : IEntityTypeConfiguration<AcademicStudent>
{
    public void Configure(EntityTypeBuilder<AcademicStudent> b)
    {
        b.ToTable("academic_students", "curriculum"); b.HasKey(x => x.Id);
        b.Property(x => x.StudentCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.UniversityId, x.StudentCode }).IsUnique();
        b.HasOne<University>().WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Restrict);
    }
}
