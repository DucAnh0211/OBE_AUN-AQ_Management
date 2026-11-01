namespace ObeAunQa.Modules.Knowledge.Application;
public interface IKnowledgePipeline
{
    Task<PipelineConfiguration> ConfigurationAsync(CancellationToken ct);
    Task<PipelineResult> ProcessAsync(string fileName, byte[] bytes, string model, bool forceOcr, CancellationToken ct);
}
