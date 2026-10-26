namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CreateProgramCourseRequest(
    string? InstitutionalCode,
    string? Name,
    int Credits,
    string? Semester,
    int? DisplayOrder);
