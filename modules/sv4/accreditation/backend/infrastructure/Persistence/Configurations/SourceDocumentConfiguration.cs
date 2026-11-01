using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ObeAunQa.Modules.Accreditation.Domain.Entities;

namespace ObeAunQa.Modules.Accreditation.Infrastructure;

public sealed class SourceDocumentConfiguration : IEntityTypeConfiguration<SourceDocument>
{
    public void Configure(EntityTypeBuilder<SourceDocument> b)
    {
        b.ToTable("source_documents", "accreditation");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(200);
        b.Property(x => x.FileName).HasMaxLength(240);
        b.Property(x => x.ContentType).HasMaxLength(120);
        b.Property(x => x.ObjectKey).HasMaxLength(300);
        b.Property(x => x.Bucket).HasMaxLength(100);
        b.Property(x => x.Sha256).HasMaxLength(64);
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => new { x.Bucket, x.ObjectKey }).IsUnique();
    }
}
