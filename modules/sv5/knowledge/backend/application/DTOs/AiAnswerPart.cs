namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record AiAnswerPart(string Text, IReadOnlyList<AiCitation> Citations);

