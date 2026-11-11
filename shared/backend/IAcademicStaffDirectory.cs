namespace ObeAunQa.SharedKernel;

/// <summary>Read-only view of identity accounts used by academic modules (no cross-context foreign keys).</summary>
public interface IAcademicStaffDirectory
{
    /// <summary>Active lecturer accounts.</summary>
    Task<IReadOnlyList<AcademicStaffMember>> SearchAsync(string? search, CancellationToken ct);
    /// <summary>Active lecturer account by id.</summary>
    Task<AcademicStaffMember?> FindAsync(long id, CancellationToken ct);
    /// <summary>Accounts of any role and status by id, for showing names next to stored user ids.</summary>
    Task<IReadOnlyList<AcademicStaffMember>> FindManyAsync(IReadOnlyCollection<long> ids, CancellationToken ct);
    /// <summary>Active accounts with the given role.</summary>
    Task<IReadOnlyList<AcademicStaffMember>> SearchAccountsAsync(string role, string? search, CancellationToken ct);
    /// <summary>Active account with the given role.</summary>
    Task<AcademicStaffMember?> FindAccountAsync(long id, string role, CancellationToken ct);
}
