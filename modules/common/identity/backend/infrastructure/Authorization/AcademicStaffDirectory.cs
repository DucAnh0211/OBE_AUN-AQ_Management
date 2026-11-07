using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Identity.Infrastructure;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Identity;

internal sealed class AcademicStaffDirectory(IdentityDbContext db) : IAcademicStaffDirectory
{
    public async Task<IReadOnlyList<AcademicStaffMember>> SearchAsync(string? search, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking().Where(u => u.Role == SystemRoles.Lecturer && u.Status == "active");
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(u => EF.Functions.ILike(u.FullName, "%" + search.Trim() + "%") || EF.Functions.ILike(u.Email, "%" + search.Trim() + "%"));
        return await query.OrderBy(u => u.FullName).ThenBy(u => u.Id).Take(50).Select(u => new AcademicStaffMember(u.Id, u.FullName, u.Email)).ToArrayAsync(ct);
    }
    public Task<AcademicStaffMember?> FindAsync(long id, CancellationToken ct) => db.Users.AsNoTracking()
        .Where(u => u.Id == id && u.Role == SystemRoles.Lecturer && u.Status == "active")
        .Select(u => new AcademicStaffMember(u.Id, u.FullName, u.Email)).SingleOrDefaultAsync(ct);
}
