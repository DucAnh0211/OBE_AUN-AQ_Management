using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ObeAunQa.Modules.Identity.Infrastructure;
internal sealed class CourseAccessRowConfiguration : IEntityTypeConfiguration<CourseAccessRow>
{
    public void Configure(EntityTypeBuilder<CourseAccessRow> entity)
    {
        entity.ToTable("program_courses", "curriculum", t => t.ExcludeFromMigrations());
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.CourseId).HasColumnName("course_id");
        entity.Property(x => x.ProgramVersionId).HasColumnName("program_version_id");
        entity.Property(x => x.ArchivedAt).HasColumnName("archived_at");
    }
}
