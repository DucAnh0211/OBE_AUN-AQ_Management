namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record GradeChangeInfo(string ComponentCode, decimal? PreviousScore, decimal? NewScore, long ActorId, DateTime ChangedAt, string Source, string? Reason = null);
