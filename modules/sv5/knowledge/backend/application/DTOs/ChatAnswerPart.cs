namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record ChatAnswerPart(string Text, IReadOnlyList<ChatCitation> Citations);

