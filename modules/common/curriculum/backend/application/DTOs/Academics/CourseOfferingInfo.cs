namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CourseOfferingInfo(long Id, long VersionId, long ProgramCourseId, string AcademicPeriod, string Code, string GroupName,
    string SubjectCode, string SubjectName, int Credits, IReadOnlyList<OfferingLecturerInfo> Lecturers, bool ReadOnly, int StudentCount,
    int? Capacity, long? TargetHomeClassId, string? TargetHomeClassCode, string GradeStatus, string? StatusNote, DateTime? PublishedAt);
