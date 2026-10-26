namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record PloCreditCheckResponse(
    long ProgramVersionId,
    int MaximumRequiredPloCount,
    bool IsValid,
    int ViolationCount,
    IReadOnlyList<CoursePloCreditCheckResponse> Courses);
