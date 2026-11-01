using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;

public sealed class EfDocumentCatalog(AccreditationDbContext db) : IDocumentCatalog
{
    public async Task<IReadOnlyList<SourceDocumentInfo>> SearchAsync(string[] terms, CancellationToken ct)
    {
        var query = db.Set<SourceDocument>().AsNoTracking().Where(x => x.DisabledAt == null);
        foreach (var term in terms)
            query = query.Where(x => x.Title.ToLower().Contains(term) || x.FileName.ToLower().Contains(term));
        var rows = await query.OrderBy(x => x.Title).ThenBy(x => x.Id).Take(101).Select(x => new SourceDocumentInfo(x.Id, x.Title, x.FileName, x.ContentType,
                x.Size, x.Sha256, x.CreatedBy, x.CreatedAt, x.DisabledAt)).ToArrayAsync(ct);
        if (rows.Length > 100) throw new DocumentException(422, "Có hơn 100 tài liệu khớp. Hãy nhập từ khóa cụ thể hơn.");
        return rows;
    }
}
