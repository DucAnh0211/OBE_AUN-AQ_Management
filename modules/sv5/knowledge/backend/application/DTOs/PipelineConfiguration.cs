namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record PipelineConfiguration(bool EmbeddingConfigured, string EmbeddingModel, bool OcrAvailable, string? Error = null, int MaxFileMb = 20);
