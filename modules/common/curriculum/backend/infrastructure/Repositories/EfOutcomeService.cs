using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfOutcomeService(CurriculumDbContext db, ICurrentUser user, IAccessControlService access,
    CourseOfferingService offeringAccess, CurriculumWriteSession writes) : IOutcomeAnalyticsService
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public async Task<OutcomeSummary> SummaryAsync(OutcomeScope scope, CancellationToken ct)
    {
        var r=await ReadAsync(scope,ct);
        return new(r.SchemaVersion,r.SourceHash,r.Status,r.Input.Scope,r.Input.Policy,r.Input.Courses,r.Issues,r.Clos,r.Plos);
    }
    public async Task<PagedResult<StudentOutcome>> StudentsAsync(OutcomeScope scope, int page, string? search, CancellationToken ct)
    {
        var q=CurriculumValidator.ListQuery(page,50,search);var r=await ReadAsync(scope,ct);
        var rows=r.CloStudents.Concat(r.PloStudents).Where(s=>q.Search==null || s.StudentCode.Contains(q.Search,StringComparison.OrdinalIgnoreCase)
            || s.Code.Contains(q.Search,StringComparison.OrdinalIgnoreCase)).OrderBy(s=>s.StudentCode).ThenBy(s=>s.ProgramCourseId).ThenBy(s=>s.OutcomeId).ToArray();
        return PagedResult<StudentOutcome>.Create(rows.Skip((q.Page-1)*50).Take(50).ToArray(),q.Page,50,rows.Length);
    }
    public async Task<object> AuditAsync(int page, CancellationToken ct)
    {
        if(!SystemRoles.IsAdmin(user.Role)) throw new AcademicAccessException();
        page=Math.Max(1,page);var query=db.Set<CurriculumAuditRecord>().AsNoTracking();
        return new {totalItems=await query.LongCountAsync(ct),page,pageSize=50,items=await query.OrderByDescending(a=>a.Id).Skip((page-1)*50).Take(50).ToArrayAsync(ct)};
    }
    public async Task<OutcomeOptions> OptionsAsync(long versionId, CancellationToken ct)
    {
        if (!await access.CanReadVersionAsync(versionId,ct)) throw new AcademicAccessException();
        if(!await db.ProgramVersions.AnyAsync(v=>v.Id==versionId&&v.ArchivedAt==null,ct)) throw new CurriculumNotFoundException("phiên bản",versionId);
        var ids = SystemRoles.IsAdmin(user.Role) ? await db.ProgramCourses.Where(c=>c.ProgramVersionId==versionId&&c.ArchivedAt==null).Select(c=>c.Id).ToArrayAsync(ct)
            : await access.GetReadableCourseIdsAsync(versionId,ct);
        var courses = await db.ProgramCourses.Where(c=>ids.Contains(c.Id)).OrderBy(c=>c.DisplayOrder).Select(c=>new OutcomeOption(c.Id,c.CourseCodeSnapshot+" · "+c.CourseNameSnapshot)).ToArrayAsync(ct);
        var offerings = await db.Set<CourseOffering>().Where(o=>o.ProgramVersionId==versionId && ids.Contains(o.ProgramCourseId)
            && (user.Role!=SystemRoles.Lecturer || o.Lecturers.Any(l=>l.UserId==user.UserId))
            && (user.Role!=SystemRoles.Student || o.GradeStatus=="published" && db.Set<CourseEnrollment>().Any(e=>e.OfferingId==o.Id&&e.Student.UserId==user.UserId)))
            .OrderBy(o=>o.AcademicPeriod).ThenBy(o=>o.Id).Select(o=>new {o.Id,o.Code,o.AcademicPeriod,o.ProgramCourse.CourseNameSnapshot}).ToArrayAsync(ct);
        var offeringIds=offerings.Select(o=>o.Id).ToArray();
        var cohorts=await db.Set<CourseEnrollment>().Where(e=>offeringIds.Contains(e.OfferingId)
            && (user.Role!=SystemRoles.Student || e.Student.UserId==user.UserId))
            .Select(e=>e.Student.CohortCode).Where(c=>c!=null && c.Trim()!="").Distinct().OrderBy(c=>c).Select(c=>c!).ToArrayAsync(ct);
        return new(courses,offerings.Select(o=>new OutcomeOption(o.Id,$"{o.Code} · {o.CourseNameSnapshot} · {o.AcademicPeriod}")).ToArray(),offerings.Select(o=>o.AcademicPeriod).Distinct().ToArray(),cohorts);
    }
    private async Task Authorize(OutcomeScope scope, bool write, CancellationToken ct)
    {
        if (scope.VersionId<=0 || scope.OfferingId is <=0 || scope.ProgramCourseId is <=0 || scope.Cohort?.Length>20
            || scope.Period!=null && OutcomeRules.PeriodOrder(scope.Period)<0)
            AcademicValidator.Invalid("scope","Phạm vi hoặc học kỳ không hợp lệ.");
        if (!await access.CanReadVersionAsync(scope.VersionId, ct)) throw new AcademicAccessException();
        if (user.Role == SystemRoles.Student && scope.Preview) throw new AcademicAccessException();
        if (write && !SystemRoles.IsAdmin(user.Role)) throw new AcademicAccessException();
        if (user.Role == SystemRoles.Lecturer)
        {
            if (scope.OfferingId is not long id) throw new AcademicAccessException();
            await offeringAccess.AccessAsync(scope.VersionId, id, false, ct);
        }
        if (scope.ProgramCourseId is long c && (!await access.CanReadCourseAsync(c, ct)
            || !await db.ProgramCourses.AnyAsync(x => x.Id == c && x.ProgramVersionId == scope.VersionId, ct))) throw new AcademicAccessException();
        if (scope.OfferingId is long o && !await db.Set<CourseOffering>().AnyAsync(x => x.Id == o && x.ProgramVersionId == scope.VersionId
            && (scope.ProgramCourseId == null || x.ProgramCourseId == scope.ProgramCourseId), ct)) throw new CurriculumNotFoundException("lớp học phần", o);
    }
    public async Task<OutcomePolicy> PolicyAsync(long versionId, CancellationToken ct)
    {
        if (!await access.CanReadVersionAsync(versionId, ct)) throw new AcademicAccessException();
        if(!await db.ProgramVersions.AnyAsync(v=>v.Id==versionId&&v.ArchivedAt==null,ct)) throw new CurriculumNotFoundException("phiên bản",versionId);
        var row = await db.Set<OutcomePolicyRecord>().AsNoTracking().Where(x => x.VersionId == versionId).OrderByDescending(x => x.Revision).FirstOrDefaultAsync(ct);
        if(row!=null) return JsonSerializer.Deserialize<OutcomePolicy>(row.Json,Json)!;
        var (academicPolicy,_) = await db.RulesAsync(await db.UniversityOfVersionAsync(versionId,ct),ct);
        return new(RetakePolicy:academicPolicy.RetakePolicy);
    }
    public async Task<OutcomePolicy> SavePolicyAsync(long versionId, OutcomePolicy request, CancellationToken ct)
    {
        await Authorize(new(versionId), true, ct); OutcomeRules.Validate(request);
        return await writes.RunAsync(async () => {
            await writes.LockVersionAsync(versionId, ct); var current = await PolicyAsync(versionId, ct);
            if (request.Revision != current.Revision) throw new CurriculumConflictException("revision_conflict", "Policy đã thay đổi. Hãy tải lại.");
            var result = request with { Revision = current.Revision + 1 };
            db.Add(new OutcomePolicyRecord { VersionId = versionId, Revision = result.Revision, Json = JsonSerializer.Serialize(result, Json), CreatedAt = DateTime.UtcNow, CreatedBy = user.UserId });
            return result;
        }, ct);
    }
    public async Task<OutcomeResult> ReadAsync(OutcomeScope scope, CancellationToken ct)
    {
        await Authorize(scope, false, ct);
        await using var transaction = db.Database.CurrentTransaction == null ? await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct) : null;
        var version = await db.ProgramVersions.AsNoTracking().Include(v => v.Program).SingleOrDefaultAsync(v => v.Id == scope.VersionId && v.ArchivedAt == null, ct)
            ?? throw new CurriculumNotFoundException("phiên bản", scope.VersionId);
        var courses = await db.ProgramCourses.AsNoTracking().Where(c => c.ProgramVersionId == scope.VersionId && c.ArchivedAt == null
            && (scope.ProgramCourseId == null || c.Id == scope.ProgramCourseId)
            && (scope.OfferingId == null || db.Set<CourseOffering>().Any(o => o.Id == scope.OfferingId && o.ProgramCourseId == c.Id))).OrderBy(c => c.Id).ToArrayAsync(ct);
        var ids = courses.Select(c => c.Id).ToArray();
        foreach (var id in ids) if (!await access.CanReadCourseAsync(id, ct)) throw new AcademicAccessException();
        var clos = await db.Clos.AsNoTracking().Where(c => ids.Contains(c.ProgramCourseId) && c.DatasetStatus != "archived").OrderBy(c => c.Id).ToArrayAsync(ct);
        var cloIds = clos.Select(c => c.Id).ToArray();
        var assessments = await db.Set<AssessmentComponent>().AsNoTracking().Where(a => ids.Contains(a.ProgramCourseId)).OrderBy(a => a.Id).ToArrayAsync(ct);
        var assessmentIds = assessments.Select(a => a.Id).ToArray();
        var maps = await db.Set<AssessmentComponentClo>().AsNoTracking().Where(m => assessmentIds.Contains(m.ComponentId)).OrderBy(m => m.ComponentId).ThenBy(m => m.CloId).ToArrayAsync(ct);
        var links = await db.CoursePlos.AsNoTracking().Where(l => ids.Contains(l.ProgramCourseId)).OrderBy(l => l.Id).ToArrayAsync(ct);
        var linkIds = links.Select(l => l.Id).ToArray();
        var cloLinks = await db.CloPlos.AsNoTracking().Where(l => linkIds.Contains(l.CoursePloId) && cloIds.Contains(l.CloId)).OrderBy(l => l.CloId).ToArrayAsync(ct);
        var offerings = await db.Set<CourseOffering>().AsNoTracking().Where(o => ids.Contains(o.ProgramCourseId)
            && (scope.OfferingId == null || o.Id == scope.OfferingId) && (scope.Period == null || o.AcademicPeriod == scope.Period)).OrderBy(o => o.Id).ToArrayAsync(ct);
        var offeringIds = offerings.Select(o => o.Id).ToArray();
        var enrollments = await db.Set<CourseEnrollment>().AsNoTracking().Include(e => e.Student).Where(e => offeringIds.Contains(e.OfferingId)
            && (scope.Cohort == null || e.Student.CohortCode == scope.Cohort)
            && (user.Role != SystemRoles.Student || e.Student.UserId == user.UserId && e.PublishedAt != null)).OrderBy(e => e.Id).ToArrayAsync(ct);
        var enrollmentIds = enrollments.Select(e => e.Id).ToArray();
        var grades = await db.Set<StudentGrade>().AsNoTracking().Where(g => enrollmentIds.Contains(g.EnrollmentId)).OrderBy(g => g.EnrollmentId).ThenBy(g => g.ComponentId).ToArrayAsync(ct);
        var plos = await db.Plos.AsNoTracking().Where(p => p.ProgramVersionId == scope.VersionId).OrderBy(p => p.Id).Select(p => new OutcomeDefinition(p.Id, p.Code, p.Statement)).ToArrayAsync(ct);
        if (scope.ProgramCourseId != null || scope.OfferingId != null) plos = plos.Where(p => links.Any(l => l.PloId == p.Id)).ToArray();
        var policy = await PolicyAsync(scope.VersionId, ct);
        var (academicPolicy, _) = await db.RulesAsync(version.Program.UniversityId,ct);
        var input = new OutcomeInput(version.Program.UniversityId, version.ProgramId, scope, policy,
            courses.Select(c => new OutcomeCourse(c.Id, c.CourseId, c.CourseCodeSnapshot, c.CourseNameSnapshot, c.Credits,
                clos.Where(x => x.ProgramCourseId == c.Id).Select(x => new OutcomeDefinition(x.Id, x.Code, x.Statement)).ToArray(),
                assessments.Where(a => a.ProgramCourseId == c.Id).Select(a => new OutcomeAssessment(a.Id, a.Code, a.Weight, a.MaxScore,
                    maps.Where(m => m.ComponentId == a.Id).ToDictionary(m => m.CloId, m => m.Weight))).ToArray(),
                links.Where(l => l.ProgramCourseId == c.Id).Select(l => new OutcomeLink(l.PloId, l.WeightCode.ToString(), cloLinks.Where(cl => cl.CoursePloId == l.Id).Select(cl => cl.CloId).Order().ToArray())).ToArray())).ToArray(),
            plos, enrollments.Select(e => {
                var o = offerings.Single(o => o.Id == e.OfferingId); var c = courses.Single(c => c.Id == e.ProgramCourseId);
                return new OutcomeAttempt(e.Id, e.StudentId, e.Student.StudentCode, e.Student.FullName, e.Student.CohortCode, o.Id, c.Id, o.AcademicPeriod, o.GradeStatus, e.Revision,
                    e.SpecialMark, e.FinalScore, e.GradePoint, c.CountsTowardGpa, e.PublishedAt != null && o.GradeStatus == "published",
                    grades.Where(g => g.EnrollmentId == e.Id).ToDictionary(g => g.ComponentId, g => scope.Preview || e.PublishedAt != null && o.GradeStatus == "published" ? (decimal?)g.Score : null),e.Passed);
            }).ToArray(),academicPolicy.RetakePolicy,academicPolicy.GpaDecimals);
        if (transaction != null) await transaction.CommitAsync(ct);
        return OutcomeRules.Calculate(input);
    }
    public async Task<OutcomeSnapshotResponse> CreateSnapshotAsync(OutcomeScope scope, CancellationToken ct)
    {
        await Authorize(scope, !scope.Preview, ct);
        if (user.Role == SystemRoles.Student) throw new AcademicAccessException();
        return await writes.RunAsync(async () => {
            await writes.LockVersionAsync(scope.VersionId,ct);
            var result = await ReadAsync(scope, ct);
            if (!scope.Preview && result.Issues.Length > 0) throw new CurriculumConflictException("outcome_not_ready", "Cấu hình OBE chưa sẵn sàng. Kiểm tra readiness.");
            var record = new OutcomeSnapshotRecord { Id = Guid.NewGuid(), VersionId = scope.VersionId, Json = JsonSerializer.Serialize(result, Json), CreatedAt = DateTime.UtcNow, CreatedBy = user.UserId };
            db.Add(record);
            return new OutcomeSnapshotResponse(record.Id, record.CreatedAt, record.CreatedBy, false, result);
        },ct);
    }
    public async Task<OutcomeSnapshotResponse> SnapshotAsync(long versionId, Guid id, CancellationToken ct)
    {
        var record = await db.Set<OutcomeSnapshotRecord>().AsNoTracking().SingleOrDefaultAsync(s => s.Id == id && s.VersionId == versionId, ct)
            ?? throw new CurriculumNotFoundException("snapshot", 0);
        var result = JsonSerializer.Deserialize<OutcomeResult>(record.Json, Json)!;
        if (user.Role == SystemRoles.Student) throw new AcademicAccessException();
        await Authorize(result.Input.Scope,false,ct);
        try {
            var current = await ReadAsync(result.Input.Scope, ct);
            return new(id, record.CreatedAt, record.CreatedBy, current.SourceHash != result.SourceHash, result);
        } catch(CurriculumNotFoundException) {
            // The source may be archived; keep authorized historical snapshots available.
            return new(id,record.CreatedAt,record.CreatedBy,true,result);
        }
    }
    public async Task<OutcomeSnapshotResponse?> LatestAsync(OutcomeScope scope, CancellationToken ct)
    {
        await Authorize(scope,false,ct);
        if(user.Role==SystemRoles.Student) throw new AcademicAccessException();
        // shortcut: scan snapshots within one version; add an indexed scope key when history grows beyond thesis-scale data.
        var rows=await db.Set<OutcomeSnapshotRecord>().AsNoTracking().Where(s=>s.VersionId==scope.VersionId).OrderByDescending(s=>s.CreatedAt).ThenByDescending(s=>s.Id).ToArrayAsync(ct);
        var row=rows.FirstOrDefault(s=>JsonSerializer.Deserialize<OutcomeResult>(s.Json,Json)!.Input.Scope==scope);
        if(row==null) return null;
        var result=await SnapshotAsync(scope.VersionId,row.Id,ct);
        if(result.Stale) throw new CurriculumConflictException("outcome_stale","Snapshot gần nhất đã cũ. Hãy tính và tạo snapshot mới.");
        return result;
    }
    public async Task<object> CompareAsync(OutcomeScope scope, string otherPeriod, CancellationToken ct)
    {
        if (scope.Period == null || OutcomeRules.PeriodOrder(otherPeriod) < 0) AcademicValidator.Invalid("period", "Chọn hai học kỳ hợp lệ.");
        if(scope.OfferingId is long offering && user.Role!=SystemRoles.Lecturer) {
            await Authorize(scope,false,ct);
            scope=scope with { OfferingId=null, ProgramCourseId=await db.Set<CourseOffering>().Where(o=>o.Id==offering&&o.ProgramVersionId==scope.VersionId).Select(o=>o.ProgramCourseId).SingleAsync(ct) };
        }
        var first = await ReadAsync(scope, ct); var second = await ReadAsync(scope with { Period = otherPeriod }, ct);
        if (first.Input.Policy != second.Input.Policy || JsonSerializer.Serialize(first.Input.Courses,Json)!=JsonSerializer.Serialize(second.Input.Courses,Json)
            || JsonSerializer.Serialize(first.Input.Plos,Json)!=JsonSerializer.Serialize(second.Input.Plos,Json))
            throw new CurriculumConflictException("policy_changed", "Định nghĩa outcome hoặc policy đã đổi trong khi so sánh. Hãy tải lại.");
        return new { first, second, differences = first.Plos.Select(p => new { p.OutcomeId, p.Code,
            percentagePoints = second.Plos.Single(s => s.OutcomeId == p.OutcomeId).AchievementRate - p.AchievementRate }) };
    }
}
