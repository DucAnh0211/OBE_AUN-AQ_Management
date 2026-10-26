namespace ObeAunQa.Modules.Curriculum.Application;

// Public, read-only contract. Reporting never queries Curriculum tables directly.
public sealed record CurriculumReportSnapshot(
    ProgramResponse Program,
    ProgramVersionResponse Version,
    IReadOnlyList<ProgramCourseResponse> Courses,
    IReadOnlyList<PloResponse> Plos,
    IReadOnlyList<CloResponse> Clos,
    IReadOnlyList<CoursePloMappingResponse> CourseMappings,
    IReadOnlyList<CloPloMappingResponse> CloMappings);
