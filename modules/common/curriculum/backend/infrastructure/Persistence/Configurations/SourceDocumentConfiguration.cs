using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class SourceDocumentConfiguration : IEntityTypeConfiguration<SourceDocument>
{
    public void Configure(EntityTypeBuilder<SourceDocument> entity)
    {
        entity.HasKey(e => e.Id).HasName("source_documents_pkey");

        entity.ToTable("source_documents", "curriculum", table => {
            table.HasCheckConstraint("source_documents_file_kind_check", "file_kind IN ('xlsx','docx','json')");
        });

        entity.HasIndex(e => new { e.FileKind, e.Sha256 }, "source_documents_file_kind_sha256_key").IsUnique();

        entity.Property(e => e.Id)
            .UseIdentityAlwaysColumn()
            .HasColumnName("id");
        entity.Property(e => e.FileKind).HasColumnName("file_kind");
        entity.Property(e => e.FileName).HasColumnName("file_name");
        entity.Property(e => e.ObjectKey).HasColumnName("object_key");
        entity.Property(e => e.Sha256)
            .HasMaxLength(64)
            .IsFixedLength()
            .HasColumnName("sha256");
    }
}
