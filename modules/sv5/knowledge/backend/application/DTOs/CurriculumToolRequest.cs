namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record CurriculumToolRequest(string Tool, long? ProgramId = null, long? VersionId = null,
    long? CourseId = null, long? CloId = null, string? Search = null, int Page = 1);
