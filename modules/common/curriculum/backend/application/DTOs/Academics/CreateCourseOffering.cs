namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CreateCourseOffering(long ProgramCourseId, string AcademicPeriod, string? GroupName = null, int? Capacity = null,
    long? TargetHomeClassId = null, IReadOnlyList<OfferingLecturerInput>? Lecturers = null);
