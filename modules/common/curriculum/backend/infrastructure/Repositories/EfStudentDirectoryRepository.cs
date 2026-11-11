using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfStudentDirectoryRepository(CurriculumDbContext db, CurriculumWriteSession writes) : IStudentDirectoryRepository
{
    private IQueryable<HomeClassInfo> ClassInfos(IQueryable<HomeClass> query) => query.Select(c => new HomeClassInfo(c.Id, c.ProgramVersionId, c.CohortCode, c.Code, c.Name,
        c.AdvisorUserId, null, db.Set<AcademicStudent>().Count(s => s.HomeClassId == c.Id)));
    private IQueryable<StudentInfo> StudentInfos(IQueryable<AcademicStudent> query) => query.Select(s => new StudentInfo(s.Id, s.StudentCode, s.FullName, s.CohortCode, s.Status,
        s.HomeClassId, db.Set<HomeClass>().Where(h => h.Id == s.HomeClassId).Select(h => h.Code).FirstOrDefault(), s.ProgramVersionId, s.UserId));
    public async Task<PagedResult<HomeClassInfo>> HomeClassesAsync(long versionId, int page, string? search, long? advisorId, CancellationToken ct)
    {
        var query = db.Set<HomeClass>().AsNoTracking().Where(c => c.ProgramVersionId == versionId && (advisorId == null || c.AdvisorUserId == advisorId));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => EF.Functions.ILike(c.Code, "%" + search + "%") || EF.Functions.ILike(c.Name, "%" + search + "%")
            || (c.CohortCode != null && EF.Functions.ILike(c.CohortCode, "%" + search + "%")));
        var total = await query.LongCountAsync(ct);
        var rows = await ClassInfos(query.OrderBy(c => c.CohortCode).ThenBy(c => c.Code).Skip((page - 1) * 50).Take(50)).ToArrayAsync(ct);
        return PagedResult<HomeClassInfo>.Create(rows, page, 50, total);
    }
    public async Task<HomeClassInfo> HomeClassAsync(long versionId, long id, CancellationToken ct) =>
        await ClassInfos(db.Set<HomeClass>().AsNoTracking().Where(c => c.Id == id && c.ProgramVersionId == versionId)).SingleOrDefaultAsync(ct)
        ?? throw new CurriculumNotFoundException("lớp sinh hoạt", id);
    public async Task<HomeClassInfo> SaveHomeClassAsync(long versionId, long? id, SaveHomeClass request, CancellationToken ct)
    {
        var saved = await writes.RunAsync(async () => {
            await writes.LockVersionAsync(versionId, ct);
            HomeClass row;
            if (id is long existing) row = await db.Set<HomeClass>().SingleOrDefaultAsync(c => c.Id == existing && c.ProgramVersionId == versionId, ct) ?? throw new CurriculumNotFoundException("lớp sinh hoạt", existing);
            else { row = new HomeClass { ProgramVersionId = versionId, CreatedAt = DateTime.UtcNow }; db.Add(row); }
            row.Code = request.Code; row.Name = request.Name; row.CohortCode = request.CohortCode; row.AdvisorUserId = request.AdvisorUserId;
            await db.SaveChangesAsync(ct); return row.Id;
        }, ct);
        return await HomeClassAsync(versionId, saved, ct);
    }
    public async Task<PagedResult<StudentInfo>> StudentsAsync(long universityId, int page, string? search, long? homeClassId, CancellationToken ct)
    {
        var query = db.Set<AcademicStudent>().AsNoTracking().Where(s => s.UniversityId == universityId && (homeClassId == null || s.HomeClassId == homeClassId));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(s => EF.Functions.ILike(s.StudentCode, "%" + search + "%") || EF.Functions.ILike(s.FullName, "%" + search + "%"));
        var total = await query.LongCountAsync(ct);
        var rows = await StudentInfos(query.OrderBy(s => s.StudentCode).Skip((page - 1) * 50).Take(50)).ToArrayAsync(ct);
        return PagedResult<StudentInfo>.Create(rows, page, 50, total);
    }
    public async Task<StudentInfo> StudentAsync(long universityId, long id, CancellationToken ct) =>
        await StudentInfos(db.Set<AcademicStudent>().AsNoTracking().Where(s => s.Id == id && s.UniversityId == universityId)).SingleOrDefaultAsync(ct)
        ?? throw new CurriculumNotFoundException("sinh viên", id);
    public Task<StudentInfo?> StudentByUserAsync(long userId, CancellationToken ct) =>
        StudentInfos(db.Set<AcademicStudent>().AsNoTracking().Where(s => s.UserId == userId)).SingleOrDefaultAsync(ct);
    public async Task<StudentInfo> UpdateStudentAsync(long universityId, long id, UpdateStudent request, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            var row = await db.Set<AcademicStudent>().SingleOrDefaultAsync(s => s.Id == id && s.UniversityId == universityId, ct) ?? throw new CurriculumNotFoundException("sinh viên", id);
            long? version = null;
            if (request.HomeClassId is long classId)
                version = await db.Set<HomeClass>().Where(c => c.Id == classId && db.ProgramVersions.Any(v => v.Id == c.ProgramVersionId && v.Program.UniversityId == universityId))
                    .Select(c => (long?)c.ProgramVersionId).SingleOrDefaultAsync(ct) ?? throw new CurriculumNotFoundException("lớp sinh hoạt của trường", classId);
            if (request.UserId is long account && await db.Set<AcademicStudent>().AnyAsync(s => s.UserId == account && s.Id != id, ct))
                throw new CurriculumConflictException("student_account_linked", "Tài khoản này đã được liên kết với hồ sơ sinh viên khác.");
            row.FullName = request.FullName; row.CohortCode = request.CohortCode; row.Status = request.Status; row.UserId = request.UserId;
            row.HomeClassId = request.HomeClassId; row.ProgramVersionId = version ?? (request.HomeClassId is null ? row.ProgramVersionId : null);
            return true;
        }, ct);
        return await StudentAsync(universityId, id, ct);
    }
    private async Task<(HomeClass Class, long UniversityId)> LockClass(long versionId, long homeClassId, CancellationToken ct)
    {
        await writes.LockVersionAsync(versionId, ct);
        var cls = await db.Set<HomeClass>().AsNoTracking().SingleOrDefaultAsync(c => c.Id == homeClassId && c.ProgramVersionId == versionId, ct)
            ?? throw new CurriculumNotFoundException("lớp sinh hoạt", homeClassId);
        return (cls, await db.UniversityOfVersionAsync(versionId, ct));
    }
    private static string? ClassProblem(AcademicStudent? student, string fullName, long homeClassId)
    {
        if (student is null) return null;
        if (student.FullName != fullName) return "Mã sinh viên đã tồn tại với họ tên khác.";
        if (student.HomeClassId is long other && other != homeClassId) return "Sinh viên đang thuộc lớp sinh hoạt khác; đổi lớp trong hồ sơ sinh viên.";
        return null;
    }
    private void Assign(AcademicStudent? student, HomeClass cls, long universityId, string code, string fullName)
    {
        if (student is null) { student = new AcademicStudent { UniversityId = universityId, StudentCode = code, FullName = fullName, CreatedAt = DateTime.UtcNow }; db.Add(student); }
        student.HomeClassId = cls.Id; student.ProgramVersionId = cls.ProgramVersionId; student.CohortCode ??= cls.CohortCode;
    }
    public async Task<StudentInfo> AddToHomeClassAsync(long versionId, long homeClassId, AddHomeClassStudent request, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            var (cls, university) = await LockClass(versionId, homeClassId, ct);
            var student = await db.Set<AcademicStudent>().SingleOrDefaultAsync(s => s.UniversityId == university && s.StudentCode == request.StudentCode, ct);
            if (ClassProblem(student, request.FullName, homeClassId) is string problem) throw new CurriculumConflictException("home_class_conflict", problem);
            Assign(student, cls, university, request.StudentCode, request.FullName);
            await db.SaveChangesAsync(ct);
            return await db.Set<AcademicStudent>().Where(s => s.UniversityId == university && s.StudentCode == request.StudentCode).Select(s => s.Id).SingleAsync(ct);
        }, ct);
        return (await StudentInfos(db.Set<AcademicStudent>().AsNoTracking().Where(s => s.Id == id)).SingleAsync(ct));
    }
    public Task<AcademicImportPreview> ImportHomeClassAsync(long versionId, long homeClassId, IReadOnlyList<AcademicImportRow> rows, bool apply, CancellationToken ct) => writes.RunAsync(async () => {
        var (cls, university) = await LockClass(versionId, homeClassId, ct);
        var checks = new List<AcademicImportRowResult>(); var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var raw in rows)
        {
            var errors = new List<string>(); var row = raw; var action = "add";
            try
            {
                var code = AcademicValidator.StudentCode(raw.StudentCode); var name = AcademicValidator.FullName(raw.FullName);
                row = raw with { StudentCode = code, FullName = name };
                if (!seen.Add(code)) errors.Add("Mã sinh viên bị trùng trong tệp.");
                var student = await db.Set<AcademicStudent>().AsNoTracking().SingleOrDefaultAsync(s => s.UniversityId == university && s.StudentCode == code, ct);
                if (ClassProblem(student, name, homeClassId) is string problem) errors.Add(problem);
                if (student?.HomeClassId == homeClassId) action = "skip";
            }
            catch (CurriculumValidationException ex) { errors.AddRange(ex.Errors.Values.SelectMany(x => x)); }
            checks.Add(new(row, errors.Count > 0 ? "error" : action, errors));
        }
        var valid = checks.All(c => c.Errors.Count == 0);
        if (!apply || !valid) return new AcademicImportPreview(valid, checks);
        var applied = 0;
        foreach (var check in checks.Where(c => c.Action == "add"))
        {
            var student = await db.Set<AcademicStudent>().SingleOrDefaultAsync(s => s.UniversityId == university && s.StudentCode == check.Row.StudentCode, ct);
            Assign(student, cls, university, check.Row.StudentCode, check.Row.FullName); applied++;
        }
        return new AcademicImportPreview(true, checks, applied);
    }, ct);
    public async Task<long?> AdvisorOfStudentAsync(long studentId, CancellationToken ct) =>
        await (from s in db.Set<AcademicStudent>() join c in db.Set<HomeClass>() on s.HomeClassId equals c.Id where s.Id == studentId select c.AdvisorUserId).SingleOrDefaultAsync(ct);
    public async Task<StudentTranscript> TranscriptAsync(long studentId, CancellationToken ct)
    {
        var student = await db.Set<AcademicStudent>().AsNoTracking().SingleOrDefaultAsync(s => s.Id == studentId, ct) ?? throw new CurriculumNotFoundException("sinh viên", studentId);
        var info = await StudentAsync(student.UniversityId, studentId, ct);
        var (policy, _) = await db.RulesAsync(student.UniversityId, ct);
        var rows = await (from e in db.Set<CourseEnrollment>().AsNoTracking()
            join o in db.Set<CourseOffering>() on e.OfferingId equals o.Id
            join c in db.ProgramCourses on e.ProgramCourseId equals c.Id
            where e.StudentId == studentId && e.PublishedAt != null && o.GradeStatus == AcademicCodes.Published
            select new { e.AcademicPeriod, c.CourseId, c.CourseCodeSnapshot, c.CourseNameSnapshot, c.Credits, c.CountsTowardGpa, o.Code,
                e.FinalScore, e.LetterGrade, e.GradePoint, e.Passed, e.AttemptKind }).ToArrayAsync(ct);
        var attempts = rows.Select(r => (Row: r, Attempt: new TranscriptAttempt(r.CourseId, r.AcademicPeriod, r.Credits, r.CountsTowardGpa, r.GradePoint, r.Passed))).ToArray();
        var chosen = attempts.GroupBy(a => a.Row.CourseId).Select(g => GpaRules.Chosen(g.Select(a => a.Attempt), policy.RetakePolicy)).OfType<TranscriptAttempt>().ToHashSet();
        var semesters = attempts.GroupBy(a => a.Row.AcademicPeriod).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => new TranscriptSemester(g.Key,
            g.OrderBy(a => a.Row.CourseCodeSnapshot).Select(a => new TranscriptRow(a.Row.AcademicPeriod, a.Row.CourseCodeSnapshot, a.Row.CourseNameSnapshot, a.Row.Credits,
                a.Row.CountsTowardGpa, a.Row.Code, a.Row.FinalScore, a.Row.LetterGrade, a.Row.GradePoint, a.Row.Passed, a.Row.AttemptKind, chosen.Contains(a.Attempt))).ToArray(),
            GpaRules.Semester(g.Select(a => a.Attempt), policy.GpaDecimals))).ToArray();
        var cumulative = GpaRules.Cumulative(attempts.Select(a => a.Attempt), policy.RetakePolicy, policy.GpaDecimals);
        return new StudentTranscript(info, semesters, cumulative, GpaRules.Classification(cumulative.Gpa), policy.RetakePolicy);
    }
}
