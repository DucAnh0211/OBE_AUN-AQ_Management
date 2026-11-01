namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record PipelineChunk(int Ordinal, string Text, string Locator, int StartOffset, float[] Embedding);
