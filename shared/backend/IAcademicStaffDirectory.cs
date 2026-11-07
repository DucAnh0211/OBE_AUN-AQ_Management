namespace ObeAunQa.SharedKernel;

public interface IAcademicStaffDirectory
{
    Task<IReadOnlyList<AcademicStaffMember>> SearchAsync(string? search, CancellationToken ct);
    Task<AcademicStaffMember?> FindAsync(long id, CancellationToken ct);
}
