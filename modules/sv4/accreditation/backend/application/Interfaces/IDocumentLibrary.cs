namespace ObeAunQa.Modules.Accreditation.Application;

// Public SV4 boundary: immutable originals and source availability. No processing here.
public interface IDocumentLibrary
{
    Task<SourceDocumentInfo> UploadAsync(string title, string fileName, byte[] bytes, long actor, CancellationToken ct);
    Task<SourceDocumentInfo?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<SourceDocumentInfo>> ListAsync(int page, CancellationToken ct);
    Task<byte[]> ReadAsync(Guid id, CancellationToken ct);
    Task DisableAsync(Guid id, long actor, CancellationToken ct);
}
