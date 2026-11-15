using Microsoft.EntityFrameworkCore;
using ObeAunQa.SharedKernel;
using ObeAunQa.Modules.Identity.Infrastructure;
namespace ObeAunQa.Modules.Identity;

public sealed class AccessControlService(IdentityDbContext db,ICurrentUser user) : IAccessControlService
{
    private bool Staff => SystemRoles.IsAdmin(user.Role) || user.Role==SystemRoles.Lecturer;
    private IQueryable<VersionAccessRow> StudentVersions => from a in db.StudentProgramAssignments
        join v in db.Set<VersionAccessRow>() on a.ProgramId equals v.ProgramId
        where a.UserId==user.UserId && v.DatasetStatus=="finalized" && v.IsCurrent && v.ArchivedAt==null select v;
    private IQueryable<CourseAccessRow> StudentCourses => from v in StudentVersions
        join c in db.Set<CourseAccessRow>() on v.Id equals c.ProgramVersionId where c.ArchivedAt==null select c;
    public Task<bool> CanReadProgramAsync(long id,CancellationToken ct) => Staff
        ? db.Set<ProgramAccessRow>().AnyAsync(p=>p.Id==id,ct)
        : user.Role==SystemRoles.Student ? StudentVersions.AnyAsync(v=>v.ProgramId==id,ct) : Task.FromResult(false);
    public Task<bool> CanReadVersionAsync(long id,CancellationToken ct) => Staff
        ? db.Set<VersionAccessRow>().AnyAsync(v=>v.Id==id,ct)
        : user.Role==SystemRoles.Student ? StudentVersions.AnyAsync(v=>v.Id==id,ct) : Task.FromResult(false);
    public Task<bool> CanReadCourseAsync(long id,CancellationToken ct) => Staff
        ? db.Set<CourseAccessRow>().AnyAsync(c=>c.Id==id,ct)
        : user.Role==SystemRoles.Student ? StudentCourses.AnyAsync(c=>c.Id==id,ct) : Task.FromResult(false);
    public Task<bool> CanEditCourseOutcomesAsync(long id,CancellationToken ct) => SystemRoles.IsAdmin(user.Role)
        ? db.Set<CourseAccessRow>().AnyAsync(c=>c.Id==id,ct) : Task.FromResult(false);
    public Task<long[]> GetReadableProgramIdsAsync(CancellationToken ct) => Staff
        ? db.Set<ProgramAccessRow>().Select(p=>p.Id).ToArrayAsync(ct)
        : user.Role==SystemRoles.Student ? StudentVersions.Select(v=>v.ProgramId).Distinct().ToArrayAsync(ct) : Task.FromResult(Array.Empty<long>());
    public Task<long[]> GetReadableCourseIdsAsync(long versionId,CancellationToken ct) => Staff
        ? db.Set<CourseAccessRow>().Where(c=>c.ProgramVersionId==versionId).Select(c=>c.Id).ToArrayAsync(ct)
        : user.Role==SystemRoles.Student ? StudentCourses.Where(c=>c.ProgramVersionId==versionId).Select(c=>c.Id).ToArrayAsync(ct) : Task.FromResult(Array.Empty<long>());
    public Task<bool> CanReadFrameworkAsync(long id,CancellationToken ct) =>
        db.Set<FrameworkAccessRow>().AnyAsync(f=>f.Id==id && (SystemRoles.IsAdmin(user.Role) || f.Status=="published"),ct);
}
