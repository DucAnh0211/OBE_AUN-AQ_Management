namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record CloPloMappingResponse(
    long CloId,
    string CloCode,
    long CoursePloId,
    long PloId,
    string PloCode);
