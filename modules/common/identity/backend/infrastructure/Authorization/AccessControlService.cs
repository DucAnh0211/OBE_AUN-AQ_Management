using Microsoft.EntityFrameworkCore;
using ObeAunQa.SharedKernel;
using ObeAunQa.Modules.Identity.Infrastructure;

namespace ObeAunQa.Modules.Identity;

public sealed class AccessControlService(IdentityDbContext db,ICurrentUser currentUser) : IAccessControlService
{
    private IQueryable<CourseAccessRow> LecturerCourses => from a in db.LecturerCourseAssignments join c in db.Set<CourseAccessRow>() on a.ProgramCourseId equals c.Id where a.UserId==currentUser.UserId select c;
    private IQueryable<VersionAccessRow> StudentVersions => from a in db.StudentProgramAssignments join v in db.Set<VersionAccessRow>() on a.ProgramId equals v.ProgramId where a.UserId==currentUser.UserId && v.DatasetStatus=="finalized" && v.IsCurrent && v.ArchivedAt==null select v;
    private IQueryable<VersionAccessRow> LecturerVersions => from c in LecturerCourses join v in db.Set<VersionAccessRow>() on c.ProgramVersionId equals v.Id select v;
    private IQueryable<CourseAccessRow> StudentCourses => from v in StudentVersions join c in db.Set<CourseAccessRow>() on v.Id equals c.ProgramVersionId where c.ArchivedAt==null select c;
    public Task<bool> CanReadProgramAsync(long id,CancellationToken ct) => currentUser.Role switch {
        SystemRoles.Admin=>Task.FromResult(true),SystemRoles.Lecturer=>LecturerVersions.AnyAsync(v=>v.ProgramId==id,ct),SystemRoles.Student=>StudentVersions.AnyAsync(v=>v.ProgramId==id,ct),_=>Task.FromResult(false)};
    public Task<bool> CanReadVersionAsync(long id,CancellationToken ct) => currentUser.Role switch {
        SystemRoles.Admin=>Task.FromResult(true),SystemRoles.Lecturer=>LecturerVersions.AnyAsync(v=>v.Id==id,ct),SystemRoles.Student=>StudentVersions.AnyAsync(v=>v.Id==id,ct),_=>Task.FromResult(false)};
    public Task<bool> CanReadCourseAsync(long id,CancellationToken ct) => currentUser.Role switch {
        SystemRoles.Admin=>Task.FromResult(true),SystemRoles.Lecturer=>LecturerCourses.AnyAsync(c=>c.Id==id,ct),SystemRoles.Student=>StudentCourses.AnyAsync(c=>c.Id==id,ct),_=>Task.FromResult(false)};
    public Task<bool> CanEditCourseOutcomesAsync(long id,CancellationToken ct)
    {
        if(currentUser.Role==SystemRoles.Admin) return Task.FromResult(true);
        if(currentUser.Role!=SystemRoles.Lecturer) return Task.FromResult(false);
        return (from c in LecturerCourses join v in db.Set<VersionAccessRow>() on c.ProgramVersionId equals v.Id
            where c.Id==id && c.ArchivedAt==null && v.DatasetStatus=="draft" && v.ArchivedAt==null select c.Id).AnyAsync(ct);
    }
    public Task<long[]> GetReadableProgramIdsAsync(CancellationToken ct) => currentUser.Role switch {
        SystemRoles.Lecturer=>LecturerVersions.Select(v=>v.ProgramId).Distinct().ToArrayAsync(ct),
        SystemRoles.Student=>StudentVersions.Select(v=>v.ProgramId).Distinct().ToArrayAsync(ct),_=>Task.FromResult(Array.Empty<long>())};
    public Task<long[]> GetReadableCourseIdsAsync(long id,CancellationToken ct) => currentUser.Role switch {
        SystemRoles.Lecturer=>LecturerCourses.Where(c=>c.ProgramVersionId==id).Select(c=>c.Id).ToArrayAsync(ct),
        SystemRoles.Student=>StudentCourses.Where(c=>c.ProgramVersionId==id).Select(c=>c.Id).ToArrayAsync(ct),_=>Task.FromResult(Array.Empty<long>())};
    public Task<bool> CanReadFrameworkAsync(long id,CancellationToken ct) => currentUser.Role switch {
        SystemRoles.Admin=>Task.FromResult(true),SystemRoles.Lecturer=>db.Set<FrameworkAccessRow>().AnyAsync(f=>f.Id==id && f.Status=="published",ct),_=>Task.FromResult(false)};
}
