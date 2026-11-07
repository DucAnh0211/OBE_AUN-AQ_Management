namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AcademicImportRow(int RowNumber, string StudentCode, string FullName,
    IReadOnlyDictionary<string, decimal?> Scores, int? ExpectedRevision = null);
