namespace ObeAunQa.Modules.Identity;

public sealed record PagedAuditLogs(IReadOnlyList<AuditLogResponse> Items, int Page, int PageSize, long TotalItems, int TotalPages);
