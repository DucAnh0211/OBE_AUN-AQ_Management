namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record AcademicImportRowResult(AcademicImportRow Row, string Action, IReadOnlyList<string> Errors);
