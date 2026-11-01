namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record SourceExcerpt(Guid DocumentId, int Ordinal, string Title, string FileName, string ContentType, string SourceHash, string ChunkHash, string Locator, int StartOffset, string Text, string SectionText, int? Page);

