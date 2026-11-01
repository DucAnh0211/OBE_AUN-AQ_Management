using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Accreditation.Domain.Entities;

namespace ObeAunQa.Modules.Accreditation.Infrastructure;

public sealed class MinioDocumentLibrary(AccreditationDbContext db, IOptions<DocumentStorageOptions> options) : IDocumentLibrary
{
    public const int MaxBytes = 20 * 1024 * 1024;
    private static readonly Dictionary<string, string> Types = new(StringComparer.OrdinalIgnoreCase) {
        [".pdf"] = "application/pdf", [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".txt"] = "text/plain", [".md"] = "text/markdown" };
    private IMinioClient Client()
    {
        var o = options.Value;
        if (!o.Configured) throw new DocumentException(503, "Chưa cấu hình MinIO trên backend.");
        var endpoint = o.Endpoint.Contains("://") ? new Uri(o.Endpoint) : new Uri($"{(o.Secure ? "https" : "http")}://{o.Endpoint}");
        return new MinioClient().WithEndpoint(endpoint.Host, endpoint.Port)
            .WithCredentials(o.AccessKey, o.SecretKey).WithSSL(endpoint.Scheme == "https").Build();
    }
    private static SourceDocumentInfo Info(SourceDocument d) => new(d.Id, d.Title, d.FileName, d.ContentType,
        d.Size, d.Sha256, d.CreatedBy, d.CreatedAt, d.DisabledAt);
    public async Task<SourceDocumentInfo> UploadAsync(string title, string fileName, byte[] bytes, long actor, CancellationToken ct)
    {
        title = title.Trim();
        fileName = Path.GetFileName(fileName.Replace('\\', '/'));
        if (title.Length is < 1 or > 200 || fileName.Length is < 1 or > 240 || fileName.Any(char.IsControl))
            throw new DocumentException(400, "Tên tài liệu (1–200 ký tự) hoặc tên file không hợp lệ.");
        if (!Types.TryGetValue(Path.GetExtension(fileName), out var contentType))
            throw new DocumentException(400, "Chỉ hỗ trợ PDF, DOCX, TXT và Markdown.");
        if (bytes.Length is 0 or > MaxBytes) throw new DocumentException(400, "File phải có dữ liệu và không vượt quá 20 MB.");
        var d = new SourceDocument { Id = Guid.NewGuid(), Title = title, FileName = fileName, Size = bytes.Length,
            ContentType = contentType, Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
            Bucket = options.Value.Bucket, CreatedBy = actor, CreatedAt = DateTime.UtcNow };
        d.ObjectKey = $"knowledge/{d.Id:N}/original{Path.GetExtension(fileName).ToLowerInvariant()}";
        using var client = Client();
        try
        {
            if (!await client.BucketExistsAsync(new BucketExistsArgs().WithBucket(d.Bucket), ct))
            {
                try { await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(d.Bucket), ct); }
                catch { if (!await client.BucketExistsAsync(new BucketExistsArgs().WithBucket(d.Bucket), ct)) throw; }
            }
            using var stream = new MemoryStream(bytes, false);
            await client.PutObjectAsync(new PutObjectArgs().WithBucket(d.Bucket).WithObject(d.ObjectKey)
                .WithStreamData(stream).WithObjectSize(bytes.Length).WithContentType(contentType), ct);
        }
        catch (OperationCanceledException) { throw; }
        catch { throw new DocumentException(503, "Không lưu được file trên MinIO. Kiểm tra kết nối và quyền bucket."); }
        try { db.Set<SourceDocument>().Add(d); await db.SaveChangesAsync(ct); }
        catch
        {
            // Best-effort compensation; keys are unique and never overwrite another upload.
            try { await client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(d.Bucket).WithObject(d.ObjectKey), CancellationToken.None); } catch { }
            throw;
        }
        return Info(d);
    }
    public async Task<SourceDocumentInfo?> FindAsync(Guid id, CancellationToken ct)
    {
        var d = await db.Set<SourceDocument>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return d is null ? null : Info(d);
    }
    public async Task<IReadOnlyList<SourceDocumentInfo>> ListAsync(int page, CancellationToken ct) =>
        (await db.Set<SourceDocument>().AsNoTracking().OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .Skip((Math.Clamp(page, 1, 100000) - 1) * 20).Take(20).ToArrayAsync(ct)).Select(Info).ToArray();
    public async Task<byte[]> ReadAsync(Guid id, CancellationToken ct)
    {
        var d = await db.Set<SourceDocument>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new DocumentException(404, "Không tìm thấy tài liệu.");
        if (d.DisabledAt is not null) throw new DocumentException(410, "Tài liệu đã bị vô hiệu hóa.");
        using var client = Client();
        using var buffer = new MemoryStream();
        try
        {
            await client.GetObjectAsync(new GetObjectArgs().WithBucket(d.Bucket).WithObject(d.ObjectKey)
                .WithCallbackStream(async (stream, token) => {
                    var block = new byte[81920];
                    int count;
                    while ((count = await stream.ReadAsync(block, token)) > 0)
                    {
                        if (buffer.Length + count > MaxBytes) throw new DocumentException(409, "Bản gốc vượt giới hạn đã lưu.");
                        await buffer.WriteAsync(block.AsMemory(0, count), token);
                    }
                }), ct);
        }
        catch (OperationCanceledException) { throw; }
        catch { throw new DocumentException(503, "Không đọc được bản gốc từ MinIO."); }
        var bytes = buffer.ToArray();
        if (bytes.Length != d.Size || !string.Equals(Convert.ToHexString(SHA256.HashData(bytes)), d.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new DocumentException(409, "Bản gốc không khớp checksum đã lưu. Không tiếp tục xử lý.");
        return bytes;
    }
    public async Task DisableAsync(Guid id, long actor, CancellationToken ct)
    {
        if (await FindAsync(id, ct) is null) throw new DocumentException(404, "Không tìm thấy tài liệu.");
        await db.Set<SourceDocument>().Where(x => x.Id == id && x.DisabledAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.DisabledAt, DateTime.UtcNow).SetProperty(x => x.DisabledBy, actor), ct);
    }
}
