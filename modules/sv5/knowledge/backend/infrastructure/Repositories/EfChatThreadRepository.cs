using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class EfChatThreadRepository(KnowledgeDbContext db) : IChatThreadRepository
{
    public async Task<long> OwnerAsync(Guid threadId, CancellationToken ct) =>
        await db.Set<ChatThread>().Where(x => x.Id == threadId).Select(x => (long?)x.OwnerId).SingleOrDefaultAsync(ct)
        ?? throw new KnowledgeException(410, "Hội thoại đã bị xóa.");
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static ChatThreadInfo Info(ChatThread t) => new(t.Id, t.Title, t.CreatedAt, t.UpdatedAt);
    private static ChatExchangeInfo Info(ChatExchange e) => new(e.Id, e.Question,
        e.AnswerJson is null ? null : JsonSerializer.Deserialize<ChatAnswer>(e.AnswerJson, Json), e.Status, e.Phase, e.Error, e.CheckpointId, e.CreatedAt);
    private async Task<ChatThread> Owned(Guid id, long owner, CancellationToken ct) =>
        await db.Set<ChatThread>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == owner, ct)
        ?? throw new KnowledgeException(404, "Không tìm thấy cuộc trò chuyện.");
    public async Task<IReadOnlyList<ChatThreadInfo>> ListAsync(long owner, int page, CancellationToken ct) =>
        (await db.Set<ChatThread>().AsNoTracking().Where(x => x.OwnerId == owner).OrderByDescending(x => x.UpdatedAt).ThenBy(x => x.Id)
            .Skip((Math.Clamp(page, 1, 10000) - 1) * 30).Take(30).ToArrayAsync(ct)).Select(Info).ToArray();
    public async Task<ChatThreadInfo> CreateAsync(long owner, CancellationToken ct)
    {
        var t = new ChatThread { Id = Guid.NewGuid(), OwnerId = owner, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Add(t); await db.SaveChangesAsync(ct); return Info(t);
    }
    public async Task<ChatThreadDetail> GetAsync(Guid id, long owner, CancellationToken ct)
    {
        var thread = await Owned(id, owner, ct);
        var messages = await db.Set<ChatExchange>().AsNoTracking().Where(x => x.ThreadId == id).OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToArrayAsync(ct);
        return new(Info(thread), messages.Select(Info).ToArray());
    }
    public async Task DeleteAsync(Guid id, long owner, CancellationToken ct)
    {
        if (await db.Set<ChatThread>().Where(x => x.Id == id && x.OwnerId == owner).ExecuteDeleteAsync(ct) == 0)
            throw new KnowledgeException(404, "Không tìm thấy cuộc trò chuyện.");
    }
    public async Task<ChatExchangeInfo> EnqueueAsync(Guid id, long owner, SendChatMessage message, CancellationToken ct)
    {
        var thread = await Owned(id, owner, ct);
        var question = message.Question?.Trim();
        if (message.RequestId == Guid.Empty || string.IsNullOrWhiteSpace(question) || question.Length > 2000)
            throw new KnowledgeException(400, "Tin nhắn cần mã yêu cầu và nội dung từ 1 đến 2.000 ký tự.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var existing = await db.Set<ChatExchange>().SingleOrDefaultAsync(x => x.Id == message.RequestId, ct);
        if (existing is not null && (existing.ThreadId != id || existing.Question != question))
            throw new KnowledgeException(409, "Mã yêu cầu đã được sử dụng cho tin nhắn khác.");
        if (existing is not null && existing.Status != "failed") return Info(existing);
        if (await db.Set<ChatExchange>().AnyAsync(x => x.ThreadId == id && (x.Status == "queued" || x.Status == "processing"), ct))
            throw new KnowledgeException(409, "Tôi đang trả lời trong cuộc trò chuyện này. Bạn chờ một chút nhé.");
        if (existing is null && await db.Set<ChatExchange>().CountAsync(x => x.ThreadId == id, ct) >= 200)
            throw new KnowledgeException(422, "Cuộc trò chuyện đã khá dài. Bạn hãy tạo cuộc trò chuyện mới để tiếp tục nhé.");
        var row = existing ?? new ChatExchange { Id = message.RequestId, ThreadId = id, Question = question, CreatedAt = DateTime.UtcNow };
        row.Status = "queued"; row.Phase = "queued"; row.Error = null; row.Lease = null; row.LeaseUntil = null; row.Attempts = 0;
        if (existing is null) db.Add(row);
        var title = thread.Title == "Cuộc trò chuyện mới" ? question[..Math.Min(100, question.Length)] : thread.Title;
        await db.Set<ChatThread>().Where(x => x.Id == id && x.OwnerId == owner)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedAt, DateTime.UtcNow).SetProperty(x => x.Title, title), ct);
        try { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); }
        catch (DbUpdateException) { throw new KnowledgeException(409, "Tin nhắn đã được gửi hoặc cuộc trò chuyện đã thay đổi. Hãy làm mới."); }
        return Info(row);
    }
    public async Task<ChatExchange?> ClaimAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        await db.Set<ChatExchange>().Where(x => x.Status == "processing" && x.LeaseUntil < now && x.Attempts >= 3)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "failed").SetProperty(x => x.Error, "Tiến trình bị gián đoạn nhiều lần. Bạn có thể bấm thử lại.").SetProperty(x => x.Lease, (Guid?)null), ct);
        var candidate = await db.Set<ChatExchange>().AsNoTracking().Where(x => x.Status == "queued" || (x.Status == "processing" && x.LeaseUntil < now && x.Attempts < 3))
            .OrderBy(x => x.CreatedAt).FirstOrDefaultAsync(ct);
        if (candidate is null) return null;
        var lease = Guid.NewGuid();
        var n = await db.Set<ChatExchange>().Where(x => x.Id == candidate.Id && (x.Status == "queued" || (x.Status == "processing" && x.LeaseUntil < now)))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "processing").SetProperty(x => x.Lease, lease).SetProperty(x => x.LeaseUntil, now.AddMinutes(10))
                .SetProperty(x => x.Attempts, x => x.Attempts + 1), ct);
        if (n == 0) return null;
        return await db.Set<ChatExchange>().AsNoTracking().SingleAsync(x => x.Id == candidate.Id, ct);
    }
    public async Task<IReadOnlyList<ChatTurn>> HistoryAsync(ChatExchange run, CancellationToken ct)
    {
        var rows = await db.Set<ChatExchange>().AsNoTracking().Where(x => x.ThreadId == run.ThreadId && x.Status == "completed" && x.CreatedAt < run.CreatedAt)
            .OrderByDescending(x => x.CreatedAt).Take(4).ToArrayAsync(ct);
        return rows.Reverse().SelectMany(x => {
            var answer = Info(x).Answer;
            var text = answer is null ? "" : answer.Notice ?? string.Join("\n", answer.Parts.Select(p => p.Text));
            return new[] { new ChatTurn("user", x.Question), new ChatTurn("assistant", text[..Math.Min(4000, text.Length)]) };
        }).ToArray();
    }
    public async Task CompleteAsync(ChatExchange run, ChatAnswer answer, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(answer, Json);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var n = await db.Set<ChatExchange>().Where(x => x.Id == run.Id && x.Status == "processing" && x.Lease == run.Lease)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "completed").SetProperty(x => x.Phase, "complete").SetProperty(x => x.AnswerJson, json)
                .SetProperty(x => x.CompletedAt, DateTime.UtcNow).SetProperty(x => x.Lease, (Guid?)null).SetProperty(x => x.LeaseUntil, (DateTime?)null), ct);
        if (n == 0) return;
        await db.Set<ChatThread>().Where(x => x.Id == run.ThreadId).ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        await tx.CommitAsync(ct);
    }
    public Task FailAsync(ChatExchange run, string error, CancellationToken ct) =>
        db.Set<ChatExchange>().Where(x => x.Id == run.Id && x.Status == "processing" && x.Lease == run.Lease)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "failed").SetProperty(x => x.Error, error)
                .SetProperty(x => x.Lease, (Guid?)null).SetProperty(x => x.LeaseUntil, (DateTime?)null), ct);
    public async Task DemandLeaseAsync(Guid threadId, Guid runId, Guid lease, CancellationToken ct)
    {
        if (!await db.Set<ChatExchange>().AnyAsync(x => x.ThreadId == threadId && x.Id == runId && x.Status == "processing" && x.Lease == lease && x.LeaseUntil > DateTime.UtcNow, ct))
            throw new KnowledgeException(410, "Tiến trình không còn hiệu lực hoặc hội thoại đã bị xóa.");
    }
    private async Task Fence(Guid runId, Guid lease, CancellationToken ct)
    {
        // Row lock inside each write transaction fences old workers and concurrent thread deletion.
        var n = await db.Set<ChatExchange>().Where(x => x.Id == runId && x.Lease == lease && x.Status == "processing" && x.LeaseUntil > DateTime.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Phase, x => x.Phase), ct);
        if (n == 0) throw new KnowledgeException(410, "Tiến trình đã kết thúc.");
    }
    public async Task<CheckpointDetail?> CheckpointAsync(Guid runId, string? checkpointId, CancellationToken ct)
    {
        var query = db.Set<ChatCheckpoint>().AsNoTracking().Where(x => x.ExchangeId == runId);
        if (!string.IsNullOrEmpty(checkpointId)) query = query.Where(x => x.CheckpointId == checkpointId);
        var row = await query.OrderByDescending(x => x.CheckpointId).FirstOrDefaultAsync(ct);
        if (row is null) return null;
        var writes = await db.Set<ChatCheckpointWrite>().AsNoTracking().Where(x => x.ExchangeId == runId && x.CheckpointId == row.CheckpointId)
            .OrderBy(x => x.TaskId).ThenBy(x => x.Index).Select(x => new CheckpointWriteValue(x.TaskId, x.Index, x.Payload)).ToArrayAsync(ct);
        return new(new(row.CheckpointId, row.ParentId, row.Payload), writes);
    }
    private static void ValidJson(string payload)
    {
        if (payload.Length > 2_000_000) throw new KnowledgeException(413, "Checkpoint quá lớn.");
        try { using var parsed = JsonDocument.Parse(payload); }
        catch (JsonException) { throw new KnowledgeException(400, "Checkpoint không hợp lệ."); }
    }
    public async Task PutCheckpointAsync(Guid runId, Guid lease, CheckpointEnvelope value, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(value.CheckpointId) || value.CheckpointId.Length > 100 || value.ParentId?.Length > 100)
            throw new KnowledgeException(400, "Mã checkpoint không hợp lệ.");
        ValidJson(value.Payload);
        await using var tx = await db.Database.BeginTransactionAsync(ct); await Fence(runId, lease, ct);
        if (!await db.Set<ChatCheckpoint>().AnyAsync(x => x.ExchangeId == runId && x.CheckpointId == value.CheckpointId, ct))
            db.Add(new ChatCheckpoint { ExchangeId = runId, CheckpointId = value.CheckpointId, ParentId = value.ParentId, Payload = value.Payload, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);
        await db.Set<ChatExchange>().Where(x => x.Id == runId).ExecuteUpdateAsync(s => s.SetProperty(x => x.CheckpointId, value.CheckpointId), ct);
        await tx.CommitAsync(ct);
    }
    public async Task PutWritesAsync(Guid runId, Guid lease, CheckpointWritesRequest value, CancellationToken ct)
    {
        if (value.Writes.Count > 100 || value.CheckpointId.Length > 100) throw new KnowledgeException(400, "Checkpoint writes vượt giới hạn.");
        await using var tx = await db.Database.BeginTransactionAsync(ct); await Fence(runId, lease, ct);
        foreach (var write in value.Writes)
        {
            if (write.TaskId.Length > 200) throw new KnowledgeException(400, "Task ID quá dài.");
            ValidJson(write.Payload);
            var existing = await db.Set<ChatCheckpointWrite>().FindAsync([runId, value.CheckpointId, write.TaskId, write.Index], ct);
            if (existing is null) db.Add(new ChatCheckpointWrite { ExchangeId = runId, CheckpointId = value.CheckpointId, TaskId = write.TaskId, Index = write.Index, Payload = write.Payload });
            else if (write.Index < 0) existing.Payload = write.Payload;
        }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
    public Task PhaseAsync(Guid runId, Guid lease, string phase, CancellationToken ct) =>
        db.Set<ChatExchange>().Where(x => x.Id == runId && x.Lease == lease && x.Status == "processing")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Phase, phase), ct);
}
