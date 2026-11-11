using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> entity)
    {
        entity.HasKey(e => e.Id).HasName("courses_pkey");

        entity.ToTable("courses", "curriculum", t => t.HasCheckConstraint("course_code_six_digits", "institutional_code ~ '^[0-9]{6}$'"));

        entity.HasIndex(e => new { e.UniversityId, e.InstitutionalCode }, "courses_university_id_institutional_code_uq")
            .IsUnique()
            .HasFilter("(institutional_code IS NOT NULL)");

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at");
        entity.Property(e => e.InstitutionalCode).HasColumnName("institutional_code").HasMaxLength(6).IsRequired();
        entity.Property(e => e.Name).HasColumnName("name");
        entity.Property(e => e.UniversityId).HasColumnName("university_id");
        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("updated_at");

        entity.HasOne(d => d.University).WithMany(p => p.Courses)
            .HasForeignKey(d => d.UniversityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("courses_university_id_fkey");

        entity.Property(e => e.DepartmentId).HasColumnName("department_id");
        entity.HasOne<OrgUnit>().WithMany().HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("courses_department_id_fkey");
    }
}
