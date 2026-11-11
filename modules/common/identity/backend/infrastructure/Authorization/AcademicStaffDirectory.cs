using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Identity.Infrastructure;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Identity;

internal sealed class AcademicStaffDirectory(IdentityDbContext db) : IAcademicStaffDirectory
{
    public Task<IReadOnlyList<AcademicStaffMember>> SearchAsync(string? search, CancellationToken ct) => SearchAccountsAsync(SystemRoles.Lecturer, search, ct);
    public Task<AcademicStaffMember?> FindAsync(long id, CancellationToken ct) => FindAccountAsync(id, SystemRoles.Lecturer, ct);
    public async Task<IReadOnlyList<AcademicStaffMember>> SearchAccountsAsync(string role, string? search, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking().Where(u => u.Role == role && u.Status == "active");
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(u => EF.Functions.ILike(u.FullName, "%" + search.Trim() + "%") || EF.Functions.ILike(u.Email, "%" + search.Trim() + "%"));
        return await query.OrderBy(u => u.FullName).ThenBy(u => u.Id).Take(50).Select(u => new AcademicStaffMember(u.Id, u.FullName, u.Email)).ToArrayAsync(ct);
    }
    public Task<AcademicStaffMember?> FindAccountAsync(long id, string role, CancellationToken ct) => db.Users.AsNoTracking()
        .Where(u => u.Id == id && u.Role == role && u.Status == "active")
        .Select(u => new AcademicStaffMember(u.Id, u.FullName, u.Email)).SingleOrDefaultAsync(ct);
    public async Task<IReadOnlyList<AcademicStaffMember>> FindManyAsync(IReadOnlyCollection<long> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return [];
        var list = ids.Distinct().ToArray();
        return await db.Users.AsNoTracking().Where(u => list.Contains(u.Id)).Select(u => new AcademicStaffMember(u.Id, u.FullName, u.Email)).ToArrayAsync(ct);
    }
}
