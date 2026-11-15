namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomeSnapshotResponse(Guid Id, DateTime CreatedAt, long CreatedBy, bool Stale, OutcomeResult Result);
