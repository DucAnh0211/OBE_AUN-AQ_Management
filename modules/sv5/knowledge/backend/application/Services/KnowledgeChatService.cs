using System.Security.Cryptography;
using System.Text;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Application;

public sealed class KnowledgeChatService(IDocumentCatalog catalog, IDocumentLibrary documents,
    IKnowledgeSearchRepository repository, IKnowledgeChatPipeline pipeline, ICurriculumChatSource? curriculum = null)
{
    public static string Key(KnowledgeChunk c) => $"{c.DocumentId:N}:{c.Ordinal}";
    public static string Hash(KnowledgeChunk c) => Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes($"{c.Locator}\n{c.StartOffset}\n{c.Text}"))).ToLowerInvariant();

    public async Task<IReadOnlyList<SourceDocumentInfo>> SearchAsync(string? keywords, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(keywords) || keywords.Length > 200)
            throw new KnowledgeException(400, "Nhập từ khóa tiêu đề/tên file, tối đa 200 ký tự.");
        var terms = keywords.Trim().ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
        if (terms.Length > 12) throw new KnowledgeException(400, "Dùng tối đa 12 từ khóa.");
        var matches = await catalog.SearchAsync(terms, ct);
        var ready = (await repository.JobsAsync(matches.Select(d => d.Id).ToArray(), ct)).Select(j => j.DocumentId).ToHashSet();
        return matches.Where(d => ready.Contains(d.Id)).ToArray();
    }

    public async Task<ChatAnswer> AskAsync(ChatRequest request, CancellationToken ct)
    {
        var question = request.Question?.Trim();
        if (string.IsNullOrWhiteSpace(question) || question.Length > 2000)
            throw new KnowledgeException(400, "Nhập câu hỏi, tối đa 2.000 ký tự.");
        var history = request.History ?? [];
        if (history.Count > 6 || history.Any(t => t.Role is not ("user" or "assistant") || string.IsNullOrWhiteSpace(t.Content) || t.Content.Length > 6000)
            || history.Sum(t => t.Content.Length) > 16000)
            throw new KnowledgeException(400, "Lịch sử hội thoại vượt giới hạn.");
        var plan = await pipeline.PlanAsync(question, history, ct);
        if (string.IsNullOrWhiteSpace(plan.Question) || plan.Question.Length > 2000 || plan.Queries is null || plan.Queries.Count > 4)
            throw new KnowledgeException(502, "Không phân tích được câu hỏi.");
        return await AskPlannedAsync(plan, ct);
    }

    public async Task<ChatAnswer> AskPlannedAsync(DocumentSearchPlan plan, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(plan.Question) || plan.Question.Length > 2000 || plan.Queries is null || plan.Queries.Count > 4)
            throw new KnowledgeException(400, "Kế hoạch tra cứu không hợp lệ.");
        var question = plan.Question;
        if (plan.Queries.Count == 0) return Empty("Bạn muốn tra cứu chủ đề hoặc quy định nào? Hãy mô tả cụ thể hơn để tôi tìm đúng tài liệu.", 0, 0);
        var found = new Dictionary<Guid, SourceDocumentInfo>();
        foreach (var keywords in plan.Queries)
        {
            foreach (var d in await SearchAsync(keywords, ct)) found.TryAdd(d.Id, d);
            // Ordered alternatives: use the first title/filename query that identifies sources.
            if (found.Count > 0) break;
        }
        var matched = found.Values.ToArray();
        if (matched.Length == 0) return Empty("Tôi chưa tìm thấy tài liệu đã xử lý có tiêu đề/tên file phù hợp. Bạn có thể nói rõ chủ đề hoặc tên quy định cần tra cứu không?", 0, 0);
        if (matched.Length > 20) return Empty("Chủ đề khớp quá nhiều tài liệu. Bạn muốn hỏi quy định nào, thuộc năm học hoặc chương trình nào?", matched.Length, 0);
        var ids = matched.Select(d => d.Id).ToArray();
        var jobs = await repository.JobsAsync(ids, ct);
        var count = jobs.Sum(j => j.ChunkCount);
        // shortcut: bounded exact cosine in Python; introduce a vector index when filtered sets routinely exceed 4,000 chunks.
        if (count > 4000) return Empty("Phạm vi tài liệu còn quá rộng. Bạn có thể nói rõ tên quy định, năm học hoặc chủ đề cụ thể hơn không?", matched.Length, 0);
        await EnsureActive(ids, ct);
        var query = await pipeline.EmbedAsync(question, ct);
        if (query.Embedding.Length is 0 or > 4096 || query.Embedding.Any(v => !float.IsFinite(v))
            || jobs.Count != ids.Length || jobs.Any(j => j.Model != query.Model || j.Dimensions != query.Embedding.Length))
            throw new KnowledgeException(409, "Nguồn và câu hỏi khác model/số chiều embedding. Cần xử lý nguồn bằng cùng model.");
        var best = new List<(KnowledgeChunk Chunk, double Score)>();
        var scanned = 0;
        for (var offset = 0; ; offset += 128)
        {
            var batch = await repository.ChunkBatchAsync(ids, offset, ct);
            if (batch.Count == 0) break;
            scanned += batch.Count;
            if (scanned > 4000) throw new KnowledgeException(422, "Phạm vi vượt 4.000 chunks.");
            await EnsureActive(batch.Select(c => c.DocumentId).Distinct().ToArray(), ct);
            var ranked = await pipeline.RankAsync(query.Embedding, batch, ct);
            var map = batch.ToDictionary(Key);
            if (ranked.Count != Math.Min(24, batch.Count) || ranked.Select(r => r.Key).Distinct().Count() != ranked.Count
                || ranked.Any(r => !map.ContainsKey(r.Key) || !double.IsFinite(r.Score) || r.Score < -1 || r.Score > 1))
                throw new KnowledgeException(502, "Kết quả semantic search không hợp lệ.");
            best.AddRange(ranked.Select(r => (map[r.Key], r.Score)));
            best = best.OrderByDescending(r => r.Score).ThenBy(r => Key(r.Chunk)).Take(24).ToList();
        }
        if (best.Count == 0) return Empty("Không có đoạn nguồn khả dụng.", matched.Length, 0);
        var sourceMap = matched.ToDictionary(d => d.Id);
        var selected = best.ToDictionary(r => Key(r.Chunk), r => r.Chunk);
        await EnsureActive(ids, ct);
        var answer = await pipeline.AnswerAsync(question, best.Select(r => new EvidenceCandidate(
            Key(r.Chunk), sourceMap[r.Chunk.DocumentId].Title, r.Chunk.Locator, r.Chunk.Text)).ToArray(), ct);
        await EnsureActive(ids, ct);
        if (answer.Parts is null || answer.Parts.Count > 12) throw new KnowledgeException(502, "Câu trả lời không hợp lệ.");
        var parts = new List<ChatAnswerPart>();
        foreach (var part in answer.Parts)
        {
            if (string.IsNullOrWhiteSpace(part.Text) || part.Text.Length > 4000 || part.Citations is null || part.Citations.Count is 0 or > 6)
                throw new KnowledgeException(502, "Câu trả lời thiếu nguồn.");
            var citations = new List<ChatCitation>();
            foreach (var reference in part.Citations)
            {
                if (!selected.TryGetValue(reference.Key, out var chunk) || string.IsNullOrWhiteSpace(reference.Quote)
                    || !chunk.Text.Contains(reference.Quote, StringComparison.Ordinal))
                    throw new KnowledgeException(502, "Trích dẫn không khớp nội dung nguồn.");
                var current = await repository.FindChunkAsync(chunk.DocumentId, chunk.Ordinal, ct);
                if (current is null || Hash(current) != Hash(chunk)) throw new KnowledgeException(409, "Nguồn đã thay đổi. Hãy hỏi lại.");
                var d = sourceMap[chunk.DocumentId]; var hash = Hash(chunk);
                var quoteIndex = chunk.Text.IndexOf(reference.Quote, StringComparison.Ordinal);
                var quoteStart = chunk.Text[..quoteIndex].EnumerateRunes().Count();
                var quoteLength = reference.Quote.EnumerateRunes().Count();
                citations.Add(new(Key(chunk), d.Id, chunk.Ordinal, d.Title, d.FileName, chunk.Locator, chunk.StartOffset,
                    reference.Quote, d.Sha256, hash, $"/knowledge/sources/{d.Id}/{chunk.Ordinal}?sourceHash={d.Sha256}&chunkHash={hash}&quoteStart={quoteStart}&quoteLength={quoteLength}"));
            }
            parts.Add(new(part.Text, citations));
        }
        await EnsureActive(ids, ct);
        return new(parts, parts.Count == 0 ? "Không tìm thấy bằng chứng đủ để trả lời trong các tài liệu đã chọn." : null,
            matched.Length, scanned, answer.RerankModel, answer.ChatModel);
    }

    public async Task<SourceExcerpt> SourceAsync(Guid id, int ordinal, string sourceHash, string chunkHash, CancellationToken ct)
    {
        var d = await Active(id, ct);
        var c = await repository.FindChunkAsync(id, ordinal, ct) ?? throw new KnowledgeException(410, "Đoạn trích không còn khả dụng.");
        if (d.Sha256 != sourceHash || Hash(c) != chunkHash) throw new KnowledgeException(409, "Nguồn khác phiên bản được trích dẫn. Hãy hỏi lại.");
        var section = await repository.SectionAsync(id, c.Locator, ct);
        var text = new List<Rune>();
        foreach (var piece in section)
        {
            while (text.Count < piece.StartOffset) text.Add(new Rune(' '));
            var runes = piece.Text.EnumerateRunes().ToArray();
            text.AddRange(runes.Skip(Math.Max(0, text.Count - piece.StartOffset)));
        }
        await Active(id, ct);
        int? page = d.ContentType == "application/pdf" && c.Locator.StartsWith("Trang ") && int.TryParse(c.Locator[6..], out var n) && n > 0 ? n : null;
        return new(d.Id, c.Ordinal, d.Title, d.FileName, d.ContentType, d.Sha256, Hash(c), c.Locator, c.StartOffset,
            c.Text, string.Concat(text.Select(r => r.ToString())), page);
    }
    public async Task<byte[]> OriginalAsync(Guid id, string sourceHash, CancellationToken ct)
    {
        var d = await Active(id, ct);
        if (d.Sha256 != sourceHash) throw new KnowledgeException(409, "Bản gốc khác nguồn được trích dẫn.");
        var bytes = await documents.ReadAsync(id, ct);
        await Active(id, ct); return bytes;
    }
    public async Task ValidateAnswerAsync(ChatAnswer answer, CancellationToken ct, long? ownerId = null)
    {
        if (answer.DatabaseSources is { Count: > 0 } sources)
        {
            if (ownerId is null || curriculum is null || sources.Count > 8) throw new KnowledgeException(502, "Nguồn dữ liệu chương trình không hợp lệ.");
            foreach (var source in sources) {
                var current = await curriculum.ReadAsync(ownerId.Value, source.Query, ct);
                if (current.Source != source) throw new KnowledgeException(409, "Dữ liệu chương trình đã thay đổi. Bạn hãy hỏi lại để tra cứu dữ liệu mới.");
            }
        }
        if (answer.Parts is null || answer.Parts.Count > 12 || answer.Notice?.Length > 6000)
            throw new KnowledgeException(502, "Phản hồi chatbot không hợp lệ.");
        foreach (var part in answer.Parts)
        {
            if (string.IsNullOrWhiteSpace(part.Text) || part.Text.Length > 4000 || part.Citations is null || part.Citations.Count is 0 or > 6)
                throw new KnowledgeException(502, "Phản hồi về tài liệu thiếu nguồn hợp lệ.");
            foreach (var citation in part.Citations)
            {
                var d = await Active(citation.DocumentId, ct);
                var chunk = await repository.FindChunkAsync(d.Id, citation.Ordinal, ct);
                if (chunk is null || d.Sha256 != citation.SourceHash || Hash(chunk) != citation.ChunkHash
                    || string.IsNullOrWhiteSpace(citation.Quote) || !chunk.Text.Contains(citation.Quote, StringComparison.Ordinal)
                    || !citation.Url.StartsWith($"/knowledge/sources/{d.Id}/{chunk.Ordinal}?sourceHash={d.Sha256}&chunkHash={Hash(chunk)}", StringComparison.Ordinal))
                    throw new KnowledgeException(409, "Nguồn đã thay đổi hoặc không còn khả dụng. Bạn hãy hỏi lại để tra cứu nguồn hiện tại.");
            }
        }
        if (answer.Parts.Count == 0 && string.IsNullOrWhiteSpace(answer.Notice)) throw new KnowledgeException(502, "Chatbot chưa trả được nội dung.");
    }
    private async Task<SourceDocumentInfo> Active(Guid id, CancellationToken ct)
    {
        var d = await documents.FindAsync(id, ct) ?? throw new KnowledgeException(404, "Không tìm thấy tài liệu.");
        if (d.DisabledAt is not null) throw new KnowledgeException(410, "Tài liệu đã bị vô hiệu hóa.");
        return d;
    }
    private async Task EnsureActive(Guid[] ids, CancellationToken ct) { foreach (var id in ids) await Active(id, ct); }
    private static ChatAnswer Empty(string notice, int documents, int chunks) => new([], notice, documents, chunks, null, null);
}
