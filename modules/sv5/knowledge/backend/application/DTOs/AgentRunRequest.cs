namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record AgentRunRequest(Guid ThreadId, Guid RunId, Guid Lease, string Question, IReadOnlyList<ChatTurn> History);

