namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record ChatThreadDetail(ChatThreadInfo Thread, IReadOnlyList<ChatExchangeInfo> Messages);

