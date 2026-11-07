namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CourseOfferingInfo(long Id, long ClassId, long VersionId, long ProgramCourseId, string AcademicPeriod,
    string Code, string ClassCode, string ClassName, string SubjectCode, string SubjectName, int Credits,
    long? LecturerId, string? LecturerName, bool ReadOnly, int StudentCount);
