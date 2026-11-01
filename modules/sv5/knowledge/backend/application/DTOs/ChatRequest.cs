namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record ChatRequest(string? Question, IReadOnlyList<ChatTurn>? History = null);

