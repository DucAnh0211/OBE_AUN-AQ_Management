namespace ObeAunQa.Modules.Accreditation.Application;

public interface IDocumentCatalog
{
    Task<IReadOnlyList<SourceDocumentInfo>> SearchAsync(string[] terms, CancellationToken ct);
}
