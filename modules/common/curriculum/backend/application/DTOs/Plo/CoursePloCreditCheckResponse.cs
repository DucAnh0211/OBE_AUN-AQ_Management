namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CoursePloCreditCheckResponse(
    long ProgramCourseId,
    string? InstitutionalCode,
    string CourseName,
    int Credits,
    int RequiredPloCount,
    int ActualPloCount,
    bool IsValid);
