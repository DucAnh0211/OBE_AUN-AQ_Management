namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record CheckpointDetail(CheckpointEnvelope Checkpoint, IReadOnlyList<CheckpointWriteValue> Writes);

