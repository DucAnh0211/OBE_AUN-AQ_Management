using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Tests;

public sealed class KnowledgeChatServiceTests
{
    [Fact]
    public async Task NoKeywordMatchNeverCallsSemanticOrAnswer()
    {
        var setup = new Fixture { Matches = false };
        var result = await setup.Service().AskAsync(new("Câu hỏi"), default);
        Assert.Empty(result.Parts); Assert.False(setup.Embedded); Assert.False(setup.Answered);
    }
    [Fact]
    public async Task RanksOnlyDocumentsSelectedByTitleBeforePublishingCitations()
    {
        var f = new Fixture(); var result = await f.Service().AskAsync(new("Câu hỏi"), default);
        Assert.Equal(new[] { f.Id }, f.RankedDocuments);
        var citation = Assert.Single(Assert.Single(result.Parts).Citations);
        Assert.Equal("150 tín chỉ", citation.Quote); Assert.Contains("chunkHash=", citation.Url);
        Assert.Equal(f.Id, citation.DocumentId);
    }
    [Fact]
    public async Task DisabledDuringGenerationNeverReturnsAnswer()
    {
        var f = new Fixture { DisableDuringAnswer = true };
        var e = await Assert.ThrowsAsync<KnowledgeException>(() => f.Service().AskAsync(new("Câu hỏi"), default));
        Assert.Equal(410, e.StatusCode);
    }
    [Fact]
    public async Task FabricatedCitationRejected()
    {
        var f = new Fixture { Fabricate = true };
        var e = await Assert.ThrowsAsync<KnowledgeException>(() => f.Service().AskAsync(new("Câu hỏi"), default));
        Assert.Equal(502, e.StatusCode);
    }
    [Fact]
    public async Task ModelMismatchStopsBeforeSemanticRanking()
    {
        var f = new Fixture { Model = "old" };
        var e = await Assert.ThrowsAsync<KnowledgeException>(() => f.Service().AskAsync(new("Câu hỏi"), default));
        Assert.Equal(409, e.StatusCode); Assert.Empty(f.RankedDocuments);
    }
    [Fact]
    public async Task StaleCitationHashAndDisabledOriginalRejected()
    {
        var f = new Fixture();
        var e = await Assert.ThrowsAsync<KnowledgeException>(() => f.Service().SourceAsync(f.Id, 1, "hash", "wrong", default));
        Assert.Equal(409, e.StatusCode);
        f.Disabled = true;
        e = await Assert.ThrowsAsync<KnowledgeException>(() => f.Service().OriginalAsync(f.Id, "hash", default));
        Assert.Equal(410, e.StatusCode);
    }
    [Fact]
    public async Task SourceReconstructionUsesUnicodeCodePointsAndRemovesOverlap()
    {
        var f = new Fixture(); f.Chunk.Text = "150 tín chỉ"; f.Chunk.StartOffset = 2;
        f.Sections = [new() { Text = "😀 150", StartOffset = 0 }, f.Chunk];
        var source = await f.Service().SourceAsync(f.Id, 1, "hash", KnowledgeChatService.Hash(f.Chunk), default);
        Assert.Equal("😀 150 tín chỉ", source.SectionText); Assert.Equal(2, source.StartOffset);
    }
    [Fact]
    public async Task DatabaseSourcesAreRecheckedForTheThreadOwnerAndRejectStaleOrFabricatedLinks()
    {
        var f = new Fixture(); var database = new DatabaseFixture();
        var service = new KnowledgeChatService(f, f, f, f, database);
        var answer = new ChatAnswer([], "PLO của phiên bản được đọc từ database.", 0, 0, null, "test", [database.Source]);
        await service.ValidateAnswerAsync(answer, default, 42);
        Assert.Equal(42, database.Owner);
        var invalid = answer with { DatabaseSources = [database.Source with { Url = "https://untrusted.example" }] };
        Assert.Equal(409, (await Assert.ThrowsAsync<KnowledgeException>(() => service.ValidateAnswerAsync(invalid, default, 42))).StatusCode);
        database.Source = database.Source with { Hash = "changed" };
        Assert.Equal(409, (await Assert.ThrowsAsync<KnowledgeException>(() => service.ValidateAnswerAsync(answer, default, 42))).StatusCode);
        database.Revoked = true;
        Assert.Equal(403, (await Assert.ThrowsAsync<KnowledgeException>(() => service.ValidateAnswerAsync(answer, default, 42))).StatusCode);
    }
    private sealed class DatabaseFixture : ICurriculumChatSource
    {
        public long Owner; public bool Revoked;
        public ChatDatabaseSource Source = new(new("plos", VersionId: 1), "hash", "PLO", "/curriculum/versions/1/plos");
        public Task<CurriculumToolResult> ReadAsync(long owner, CurriculumToolRequest query, CancellationToken ct) {
            Owner = owner;
            if (Revoked) throw new KnowledgeException(403, "Access revoked");
            return Task.FromResult(new CurriculumToolResult(System.Text.Json.JsonSerializer.SerializeToElement(new { items = Array.Empty<string>() }), Source));
        }
    }
    private sealed class Fixture : IDocumentCatalog, IDocumentLibrary, IKnowledgeSearchRepository, IKnowledgeChatPipeline
    {
        public Guid Id = Guid.NewGuid(); public bool Matches = true, Disabled, Embedded, Answered, DisableDuringAnswer, Fabricate;
        public string Model = "test"; public Guid[] RankedDocuments = [];
        public KnowledgeChunk Chunk; public IReadOnlyList<KnowledgeChunk>? Sections;
        public Fixture() { Chunk = new() { DocumentId = Id, Ordinal = 1, Text = "Hoàn thành 150 tín chỉ.", Locator = "Trang 2", Embedding = [1, 0] }; }
        public KnowledgeChatService Service() => new(this, this, this, this);
        SourceDocumentInfo Info() => new(Id, "Quy chế", "quyche.pdf", "application/pdf", 1, "hash", 1, DateTime.UtcNow, Disabled ? DateTime.UtcNow : null);
        public Task<IReadOnlyList<SourceDocumentInfo>> SearchAsync(string[] terms, CancellationToken ct) => Task.FromResult<IReadOnlyList<SourceDocumentInfo>>(Matches ? [Info()] : []);
        public Task<SourceDocumentInfo?> FindAsync(Guid id, CancellationToken ct) => Task.FromResult<SourceDocumentInfo?>(Info());
        public Task<IReadOnlyList<IngestionJob>> JobsAsync(Guid[] ids, CancellationToken ct) => Task.FromResult<IReadOnlyList<IngestionJob>>(ids.Length == 0 ? [] : [new() { DocumentId = Id, Status = "ready", Model = Model, Dimensions = 2, ChunkCount = 1 }]);
        public Task<IReadOnlyList<KnowledgeChunk>> ChunkBatchAsync(Guid[] ids, int offset, CancellationToken ct) => Task.FromResult<IReadOnlyList<KnowledgeChunk>>(offset == 0 ? [Chunk] : []);
        public Task<KnowledgeChunk?> FindChunkAsync(Guid id, int ordinal, CancellationToken ct) => Task.FromResult<KnowledgeChunk?>(Chunk);
        public Task<IReadOnlyList<KnowledgeChunk>> SectionAsync(Guid id, string locator, CancellationToken ct) => Task.FromResult(Sections ?? (IReadOnlyList<KnowledgeChunk>)[Chunk]);
        public Task<DocumentSearchPlan> PlanAsync(string question, IReadOnlyList<ChatTurn> history, CancellationToken ct) => Task.FromResult(new DocumentSearchPlan(question, ["quy chế"]));
        public Task<QueryEmbedding> EmbedAsync(string question, CancellationToken ct) { Embedded = true; return Task.FromResult(new QueryEmbedding("test", [1, 0])); }
        public Task<IReadOnlyList<RankedChunk>> RankAsync(float[] query, IReadOnlyList<KnowledgeChunk> chunks, CancellationToken ct)
        { RankedDocuments = chunks.Select(c => c.DocumentId).ToArray(); return Task.FromResult<IReadOnlyList<RankedChunk>>([new(KnowledgeChatService.Key(Chunk), 1)]); }
        public Task<AiAnswer> AnswerAsync(string question, IReadOnlyList<EvidenceCandidate> candidates, CancellationToken ct)
        { Answered = true; if (DisableDuringAnswer) Disabled = true; return Task.FromResult(new AiAnswer([new("Cần 150 tín chỉ.", [new(KnowledgeChatService.Key(Chunk), Fabricate ? "999 tín chỉ" : "150 tín chỉ")])], "rerank", "chat")); }
        public Task<byte[]> ReadAsync(Guid id, CancellationToken ct) => Task.FromResult(new byte[] { 65 });
        public Task DisableAsync(Guid id, long actor, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<SourceDocumentInfo>> ListAsync(int page, CancellationToken ct) => throw new NotSupportedException();
        public Task<SourceDocumentInfo> UploadAsync(string title, string fileName, byte[] bytes, long actor, CancellationToken ct) => throw new NotSupportedException();
    }
}
