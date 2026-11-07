namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AcademicImportPreview(bool CanApply, IReadOnlyList<AcademicImportRowResult> Rows, int AppliedRows = 0);
