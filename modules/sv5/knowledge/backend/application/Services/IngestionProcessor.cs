using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Application;

public sealed class IngestionProcessor(IDocumentLibrary documents, IKnowledgePipeline pipeline, IKnowledgeRepository repository)
{
    public async Task ProcessAsync(IngestionJob job, CancellationToken ct)
    {
        var document = await documents.FindAsync(job.DocumentId, ct);
        if (document is null || document.DisabledAt is not null) throw new KnowledgeException(410, "Nguồn đã bị vô hiệu hóa.");
        var bytes = await documents.ReadAsync(document.Id, ct);
        var result = await pipeline.ProcessAsync(document.FileName, bytes, job.Model, job.ForceOcr, ct);
        if (result.Model != job.Model || result.Chunks.Count is 0 or > 2000 || result.PipelineVersion.Length > 100)
            throw new KnowledgeException(502, "Kết quả AI không khớp model hoặc giới hạn xử lý.");
        var dimension = result.Chunks[0].Embedding.Length;
        if (dimension is <= 0 or > 4096 || result.Chunks.Where((c, i) => c.Ordinal != i + 1 || c.Text.Length is 0 or > 2400
            || c.Locator.Length > 100 || c.StartOffset < 0 || c.Embedding.Length != dimension || c.Embedding.Any(v => !float.IsFinite(v))).Any())
            throw new KnowledgeException(502, "Chunk hoặc embedding từ AI service không hợp lệ.");
        var chunks = result.Chunks.Select(c => new KnowledgeChunk { DocumentId = document.Id, Ordinal = c.Ordinal,
            Text = c.Text, Locator = c.Locator, StartOffset = c.StartOffset, Embedding = c.Embedding }).ToArray();
        if ((await documents.FindAsync(document.Id, ct))?.DisabledAt is not null) throw new KnowledgeException(410, "Nguồn đã bị vô hiệu hóa.");
        job.PipelineVersion = result.PipelineVersion;
        await repository.CompleteAsync(job, chunks, result.Warning, ct);
    }
}
