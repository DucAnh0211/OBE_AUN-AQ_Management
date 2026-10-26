using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> entity)
    {
        entity.HasKey(e => e.Id).HasName("import_batches_pkey");

        entity.ToTable("import_batches", "curriculum");

        entity.HasIndex(e => new { e.ProgramVersionId, e.DatasetSha256 }, "import_batches_program_version_id_dataset_sha256_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.DatasetDocumentId).HasColumnName("dataset_document_id");
        entity.Property(e => e.DatasetSha256)
            .HasMaxLength(64)
            .IsFixedLength()
            .HasColumnName("dataset_sha256");
        entity.Property(e => e.ImportedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("imported_at");
        entity.Property(e => e.ProgramVersionId).HasColumnName("program_version_id");
        entity.Property(e => e.SyllabusDocumentId).HasColumnName("syllabus_document_id");
        entity.Property(e => e.WorkbookDocumentId).HasColumnName("workbook_document_id");

        entity.HasOne(d => d.DatasetDocument).WithMany(p => p.ImportBatchDatasetDocuments)
            .HasForeignKey(d => d.DatasetDocumentId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("import_batches_dataset_document_id_fkey");

        entity.HasOne(d => d.ProgramVersion).WithMany(p => p.ImportBatches)
            .HasForeignKey(d => d.ProgramVersionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("import_batches_program_version_id_fkey");

        entity.HasOne(d => d.SyllabusDocument).WithMany(p => p.ImportBatchSyllabusDocuments)
            .HasForeignKey(d => d.SyllabusDocumentId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("import_batches_syllabus_document_id_fkey");

        entity.HasOne(d => d.WorkbookDocument).WithMany(p => p.ImportBatchWorkbookDocuments)
            .HasForeignKey(d => d.WorkbookDocumentId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("import_batches_workbook_document_id_fkey");
    }
}
