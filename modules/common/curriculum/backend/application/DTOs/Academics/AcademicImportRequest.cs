namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AcademicImportRequest(string Mode, string AcademicPeriod, int SchemeRevision,
    IReadOnlyList<AcademicImportRow> Rows, bool OverwriteExisting = false, long? OfferingId = null);
