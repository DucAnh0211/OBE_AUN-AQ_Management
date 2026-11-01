namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record PipelineResult(string Model, string PipelineVersion, IReadOnlyList<PipelineChunk> Chunks, string? Warning);
