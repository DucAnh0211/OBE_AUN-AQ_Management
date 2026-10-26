namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record UpdateProgramCourseRequest(
    string? InstitutionalCode,
    string? Name,
    int Credits,
    string? Semester,
    int DisplayOrder);
