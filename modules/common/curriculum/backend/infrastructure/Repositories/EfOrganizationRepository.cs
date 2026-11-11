using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfOrganizationRepository(CurriculumDbContext db, CurriculumWriteSession writes) : IOrganizationRepository
{
    private static OrgUnitInfo Info(OrgUnit u) => new(u.Id, u.UniversityId, u.ParentId, u.Kind, u.Code, u.Name, u.ArchivedAt != null);
    public async Task<IReadOnlyList<OrgUnitInfo>> OrgUnitsAsync(long universityId, bool includeArchived, CancellationToken ct) =>
        (await db.Set<OrgUnit>().AsNoTracking().Where(u => u.UniversityId == universityId && (includeArchived || u.ArchivedAt == null))
            .OrderBy(u => u.Kind == AcademicCodes.Department).ThenBy(u => u.Code).ToArrayAsync(ct)).Select(Info).ToArray();
    public async Task<OrgUnitInfo> SaveOrgUnitAsync(long universityId, long? id, SaveOrgUnit request, CancellationToken ct)
    {
        var saved = await writes.RunAsync(async () => {
            if (request.ParentId is long parent && !await db.Set<OrgUnit>().AnyAsync(u => u.Id == parent && u.UniversityId == universityId
                    && u.Kind == AcademicCodes.Faculty && u.ArchivedAt == null && u.Id != id, ct))
                throw new CurriculumNotFoundException("khoa đang hoạt động của trường", parent);
            OrgUnit row;
            if (id is long existing)
            {
                row = await db.Set<OrgUnit>().SingleOrDefaultAsync(u => u.Id == existing && u.UniversityId == universityId, ct) ?? throw new CurriculumNotFoundException("đơn vị", existing);
                if (row.Kind != request.Kind && await db.Set<OrgUnit>().AnyAsync(u => u.ParentId == row.Id, ct))
                    throw new CurriculumConflictException("org_unit_has_children", "Khoa đang có bộ môn trực thuộc, không thể đổi thành bộ môn.");
            }
            else { row = new OrgUnit { UniversityId = universityId, CreatedAt = DateTime.UtcNow }; db.Add(row); }
            row.Kind = request.Kind; row.Code = request.Code; row.Name = request.Name; row.ParentId = request.ParentId;
            await db.SaveChangesAsync(ct); return row;
        }, ct);
        return Info(saved);
    }
    public Task ArchiveOrgUnitAsync(long universityId, long id, CancellationToken ct) => writes.RunAsync(async () => {
        var row = await db.Set<OrgUnit>().SingleOrDefaultAsync(u => u.Id == id && u.UniversityId == universityId, ct) ?? throw new CurriculumNotFoundException("đơn vị", id);
        if (await db.Set<OrgUnit>().AnyAsync(u => u.ParentId == id && u.ArchivedAt == null, ct))
            throw new CurriculumConflictException("org_unit_has_children", "Cần lưu trữ các bộ môn trực thuộc trước khi lưu trữ khoa.");
        row.ArchivedAt ??= DateTime.UtcNow; return true;
    }, ct);
    public async Task<IReadOnlyList<LecturerProfileInfo>> ProfilesAsync(IReadOnlyCollection<long> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToArray();
        return await (from p in db.Set<LecturerProfile>().AsNoTracking()
            join u in db.Set<OrgUnit>() on p.DepartmentId equals u.Id into units
            from u in units.DefaultIfEmpty()
            where ids.Contains(p.UserId)
            select new LecturerProfileInfo(p.UserId, "", "", p.LecturerCode, p.DepartmentId, u == null ? null : u.Name, p.AcademicTitle, p.Degree)).ToArrayAsync(ct);
    }
    public Task SaveProfileAsync(long userId, SaveLecturerProfile request, CancellationToken ct) => writes.RunAsync(async () => {
        if (request.DepartmentId is long d && !await db.Set<OrgUnit>().AnyAsync(u => u.Id == d && u.Kind == AcademicCodes.Department && u.ArchivedAt == null, ct))
            throw new CurriculumNotFoundException("bộ môn đang hoạt động", d);
        var row = await db.Set<LecturerProfile>().SingleOrDefaultAsync(p => p.UserId == userId, ct);
        if (row is null) { row = new LecturerProfile { UserId = userId }; db.Add(row); }
        row.LecturerCode = request.LecturerCode; row.DepartmentId = request.DepartmentId; row.AcademicTitle = request.AcademicTitle; row.Degree = request.Degree;
        row.UpdatedAt = DateTime.UtcNow; return true;
    }, ct);
    public async Task<GradingPolicyInfo> PolicyAsync(long universityId, CancellationToken ct)
    {
        var (policy, bands) = await db.RulesAsync(universityId, ct);
        return GradingQueries.Info(policy, bands);
    }
    public async Task<GradingPolicyInfo> SavePolicyAsync(long universityId, SaveGradingPolicy request, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            var row = await db.Set<GradingPolicy>().SingleOrDefaultAsync(p => p.UniversityId == universityId, ct);
            if (row is null) { row = new GradingPolicy { UniversityId = universityId }; db.Add(row); }
            row.CourseScoreDecimals = request.CourseScoreDecimals; row.MinComponents = request.MinComponents; row.MinFinalWeight = request.MinFinalWeight;
            row.RetakePolicy = request.RetakePolicy; row.GpaDecimals = request.GpaDecimals; row.UpdatedAt = DateTime.UtcNow;
            await db.Set<GradeScaleBand>().Where(b => b.UniversityId == universityId).ExecuteDeleteAsync(ct);
            db.AddRange(request.Bands.OrderByDescending(b => b.MinScore).Select((b, i) => new GradeScaleBand { UniversityId = universityId, Letter = b.Letter,
                MinScore = b.MinScore, Point4 = b.Point4, IsPassing = b.IsPassing, DisplayOrder = i + 1 }));
            return true;
        }, ct);
        return await PolicyAsync(universityId, ct);
    }
    public Task AssignFacultyAsync(long programId, long? facultyId, CancellationToken ct) => writes.RunAsync(async () => {
        var program = await db.Programs.SingleOrDefaultAsync(p => p.Id == programId && p.ArchivedAt == null, ct) ?? throw new CurriculumNotFoundException("chương trình đào tạo", programId);
        if (facultyId is long f && !await db.Set<OrgUnit>().AnyAsync(u => u.Id == f && u.UniversityId == program.UniversityId && u.Kind == AcademicCodes.Faculty && u.ArchivedAt == null, ct))
            throw new CurriculumNotFoundException("khoa đang hoạt động của trường", f);
        program.FacultyId = facultyId; program.UpdatedAt = DateTime.UtcNow; return true;
    }, ct);
    public Task AssignCourseCatalogAsync(long versionId, long programCourseId, UpdateCourseCatalog request, CancellationToken ct) => writes.RunAsync(async () => {
        await writes.LockVersionAsync(versionId, ct);
        var course = await db.ProgramCourses.Include(c => c.Course).SingleOrDefaultAsync(c => c.Id == programCourseId && c.ProgramVersionId == versionId && c.ArchivedAt == null, ct)
            ?? throw new CurriculumNotFoundException("học phần đang hoạt động", programCourseId);
        if (request.DepartmentId is long d && !await db.Set<OrgUnit>().AnyAsync(u => u.Id == d && u.UniversityId == course.Course.UniversityId && u.Kind == AcademicCodes.Department && u.ArchivedAt == null, ct))
            throw new CurriculumNotFoundException("bộ môn đang hoạt động của trường", d);
        if (course.CountsTowardGpa != request.CountsTowardGpa && await db.Set<CourseEnrollment>().AnyAsync(e => e.ProgramCourseId == programCourseId && e.PublishedAt != null, ct))
            throw new CurriculumConflictException("gpa_flag_locked", "Học phần đã có điểm công bố, không thể đổi cách tính GPA.");
        course.Course.DepartmentId = request.DepartmentId; course.Course.UpdatedAt = DateTime.UtcNow;
        course.CountsTowardGpa = request.CountsTowardGpa; course.UpdatedAt = DateTime.UtcNow; return true;
    }, ct);
}
