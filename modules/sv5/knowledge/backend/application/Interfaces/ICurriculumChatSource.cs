namespace ObeAunQa.Modules.Knowledge.Application;

public interface ICurriculumChatSource
{
    Task<CurriculumToolResult> ReadAsync(long ownerId, CurriculumToolRequest query, CancellationToken ct);
}
