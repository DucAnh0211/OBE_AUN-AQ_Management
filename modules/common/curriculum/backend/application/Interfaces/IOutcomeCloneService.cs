namespace ObeAunQa.Modules.Curriculum.Application;

public interface IOutcomeCloneService
{
    Task CopyAsync(long targetVersionId, long sourceVersionId, CancellationToken ct);
}
