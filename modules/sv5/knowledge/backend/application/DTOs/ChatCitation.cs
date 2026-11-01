namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record ChatCitation(string Key, Guid DocumentId, int Ordinal, string Title, string FileName, string Locator, int StartOffset, string Quote, string SourceHash, string ChunkHash, string Url);

