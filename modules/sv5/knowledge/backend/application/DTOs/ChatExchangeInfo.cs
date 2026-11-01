namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record ChatExchangeInfo(Guid Id, string Question, ChatAnswer? Answer, string Status, string Phase, string? Error, string? CheckpointId, DateTime CreatedAt);

