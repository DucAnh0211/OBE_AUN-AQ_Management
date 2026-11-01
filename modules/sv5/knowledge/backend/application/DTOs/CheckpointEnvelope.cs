namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record CheckpointEnvelope(string CheckpointId, string? ParentId, string Payload);

