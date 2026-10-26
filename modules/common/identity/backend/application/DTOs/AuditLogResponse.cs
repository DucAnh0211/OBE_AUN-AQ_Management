namespace ObeAunQa.Modules.Identity;

public sealed record AuditLogResponse(long Id, long? ActorUserId, string Action, string TargetType, string? TargetId, string Details, string? IpAddress, DateTime CreatedAt);
