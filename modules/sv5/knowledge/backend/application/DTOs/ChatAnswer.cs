namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record ChatAnswer(IReadOnlyList<ChatAnswerPart> Parts, string? Notice, int DocumentCount, int ChunkCount, string? RerankModel, string? ChatModel,
    IReadOnlyList<ChatDatabaseSource>? DatabaseSources = null);

