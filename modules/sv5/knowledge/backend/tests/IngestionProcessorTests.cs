using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.Modules.Knowledge.Domain;
using ObeAunQa.Modules.Knowledge.Infrastructure;

namespace ObeAunQa.Modules.Knowledge.Tests;

public sealed class IngestionProcessorTests
{
    [Fact]
    public void EfModelMatchesMigrations()
    {
        using var db = new KnowledgeDbContext(new DbContextOptionsBuilder<KnowledgeDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=test").Options);
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.Equal("real[]", db.Model.FindEntityType(typeof(KnowledgeChunk))!.FindProperty(nameof(KnowledgeChunk.Embedding))!.GetColumnType());
    }

    [Fact]
    public async Task DisabledSourceNeverCallsPython()
    {
        var documents = new Documents { Disabled = true };
        var pipeline = new Pipeline(); var repository = new Repository();
        await Assert.ThrowsAsync<KnowledgeException>(() => new IngestionProcessor(documents, pipeline, repository).ProcessAsync(Job(), default));
        Assert.False(pipeline.Called); Assert.False(repository.Completed);
    }

    [Fact]
    public async Task InvalidEmbeddingNeverPublishesChunks()
    {
        var pipeline = new Pipeline { Vector = [float.NaN] }; var repository = new Repository();
        await Assert.ThrowsAsync<KnowledgeException>(() => new IngestionProcessor(new Documents(), pipeline, repository).ProcessAsync(Job(), default));
        Assert.False(repository.Completed);
    }

    [Fact]
    public async Task SourceDisabledDuringPythonCallNeverPublishes()
    {
        var documents = new Documents(); var repository = new Repository();
        var pipeline = new Pipeline { AfterCall = () => documents.Disabled = true };
        await Assert.ThrowsAsync<KnowledgeException>(() => new IngestionProcessor(documents, pipeline, repository).ProcessAsync(Job(), default));
        Assert.False(repository.Completed);
    }

    [Fact]
    public async Task PublishesValidatedResultWithSourceAndPipelineVersion()
    {
        var repository = new Repository(); var job = Job();
        await new IngestionProcessor(new Documents(), new Pipeline(), repository).ProcessAsync(job, default);
        Assert.True(repository.Completed);
        Assert.Equal("python-test-v1", job.PipelineVersion);
        Assert.Equal(job.DocumentId, Assert.Single(repository.Chunks).DocumentId);
    }

    private static IngestionJob Job() => new() { DocumentId = Guid.Empty, Model = "test-model" };
    private sealed class Documents : IDocumentLibrary
    {
        public bool Disabled;
        public Task<SourceDocumentInfo?> FindAsync(Guid id, CancellationToken ct) => Task.FromResult<SourceDocumentInfo?>(new(id, "Title", "test.txt", "text/plain", 1, "hash", 1, DateTime.UtcNow, Disabled ? DateTime.UtcNow : null));
        public Task<byte[]> ReadAsync(Guid id, CancellationToken ct) => Task.FromResult(new byte[] { 65 });
        public Task DisableAsync(Guid id, long actor, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<SourceDocumentInfo>> ListAsync(int page, CancellationToken ct) => throw new NotSupportedException();
        public Task<SourceDocumentInfo> UploadAsync(string title, string fileName, byte[] bytes, long actor, CancellationToken ct) => throw new NotSupportedException();
    }
    private sealed class Pipeline : IKnowledgePipeline
    {
        public bool Called;
        public float[] Vector = [1, 0, 0];
        public Action? AfterCall;
        public Task<PipelineConfiguration> ConfigurationAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<PipelineResult> ProcessAsync(string fileName, byte[] bytes, string model, bool forceOcr, CancellationToken ct)
        {
            Called = true; AfterCall?.Invoke();
            return Task.FromResult(new PipelineResult(model, "python-test-v1", [new(1, "Test content", "Trang 1", 0, Vector)], null));
        }
    }
    private sealed class Repository : IKnowledgeRepository
    {
        public bool Completed;
        public IReadOnlyList<KnowledgeChunk> Chunks = [];
        public Task CompleteAsync(IngestionJob job, IReadOnlyList<KnowledgeChunk> chunks, string? warning, CancellationToken ct) { Completed = true; Chunks = chunks; return Task.CompletedTask; }
        public Task<IngestionJob?> FindAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<IngestionJob>> ListAsync(Guid[] ids, CancellationToken ct) => throw new NotSupportedException();
        public Task QueueAsync(Guid id, string model, bool forceOcr, CancellationToken ct) => throw new NotSupportedException();
        public Task<IngestionJob?> ClaimAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task FailAsync(IngestionJob job, string message, CancellationToken ct) => throw new NotSupportedException();
        public Task DisableAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<KnowledgeChunk>> ChunksAsync(Guid id, int page, CancellationToken ct) => throw new NotSupportedException();
    }
}
