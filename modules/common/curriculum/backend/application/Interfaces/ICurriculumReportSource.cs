namespace ObeAunQa.Modules.Curriculum.Application;

// Public, read-only contract. Reporting never queries Curriculum tables directly.
public interface ICurriculumReportSource
{
    Task<CurriculumReportSnapshot?> ReadAsync(long versionId, CancellationToken cancellationToken);
}
