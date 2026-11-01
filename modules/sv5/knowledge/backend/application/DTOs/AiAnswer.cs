namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record AiAnswer(IReadOnlyList<AiAnswerPart> Parts, string RerankModel, string ChatModel);

