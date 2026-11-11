namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record LecturerProfileInfo(long UserId, string FullName, string Email, string? LecturerCode, long? DepartmentId,
    string? DepartmentName, string? AcademicTitle, string? Degree);
