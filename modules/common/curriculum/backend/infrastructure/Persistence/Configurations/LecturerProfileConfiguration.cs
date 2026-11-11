using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class LecturerProfileConfiguration : IEntityTypeConfiguration<LecturerProfile>
{
    public void Configure(EntityTypeBuilder<LecturerProfile> b)
    {
        b.ToTable("lecturer_profiles", "curriculum"); b.HasKey(x => x.UserId); b.Property(x => x.UserId).ValueGeneratedNever();
        b.Property(x => x.LecturerCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.AcademicTitle).HasMaxLength(10); b.Property(x => x.Degree).HasMaxLength(10);
        b.HasIndex(x => x.LecturerCode).IsUnique();
        b.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
