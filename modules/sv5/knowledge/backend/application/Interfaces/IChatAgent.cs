namespace ObeAunQa.Modules.Knowledge.Application;
public interface IChatAgent
{
    Task<ChatAnswer> RunAsync(AgentRunRequest request, CancellationToken ct);
}

