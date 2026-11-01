namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record CheckpointWritesRequest(string CheckpointId, IReadOnlyList<CheckpointWriteValue> Writes);

