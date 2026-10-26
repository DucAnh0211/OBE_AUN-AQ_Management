using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class SourceCourseRowConfiguration : IEntityTypeConfiguration<SourceCourseRow>
{
    public void Configure(EntityTypeBuilder<SourceCourseRow> entity)
    {
        entity.HasKey(e => new { e.ImportBatchId, e.SourceRow }).HasName("source_course_rows_pkey");

        entity.ToTable("source_course_rows", "curriculum");

        entity.Property(e => e.ImportBatchId).HasColumnName("import_batch_id");
        entity.Property(e => e.SourceRow).HasColumnName("source_row");
        entity.Property(e => e.RawData)
            .HasColumnType("jsonb")
            .HasColumnName("raw_data");

        entity.HasOne(d => d.ImportBatch).WithMany(p => p.SourceCourseRows)
            .HasForeignKey(d => d.ImportBatchId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("source_course_rows_import_batch_id_fkey");
    }
}
