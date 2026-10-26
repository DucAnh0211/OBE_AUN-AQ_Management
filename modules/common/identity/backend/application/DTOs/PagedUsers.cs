namespace ObeAunQa.Modules.Identity;

public sealed record PagedUsers(IReadOnlyList<UserProfile> Items, int Page, int PageSize, long TotalItems, int TotalPages);
