namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record NormalizedCourse(
    string? InstitutionalCode,
    string Name,
    int Credits,
    string Semester,
    int? DisplayOrder);
