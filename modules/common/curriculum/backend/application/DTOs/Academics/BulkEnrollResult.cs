namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record BulkEnrollResult(int Added, int Skipped, IReadOnlyList<string> Messages);
