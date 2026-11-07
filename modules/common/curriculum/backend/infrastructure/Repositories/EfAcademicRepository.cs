using Microsoft.EntityFrameworkCore;
using ObeAunQa.SharedKernel;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfAcademicRepository(CurriculumDbContext db, CurriculumWriteSession writes, ICurrentUser user) : IAcademicRepository
{
    public async Task<IReadOnlyList<string>> PeriodsAsync(long courseId, CancellationToken ct) =>
        await db.Set<CourseEnrollment>().AsNoTracking().Where(e => e.ProgramCourseId == courseId)
            .Select(e => e.AcademicPeriod).Distinct().OrderBy(p => p).ToArrayAsync(ct);
    private async Task<ProgramCourse> LockCourse(long versionId, long courseId, long offeringId, CancellationToken ct)
    {
        await writes.LockVersionAsync(versionId, ct);
        var offering = await db.Set<CourseOffering>().AsNoTracking().SingleOrDefaultAsync(o => o.Id == offeringId && o.ProgramVersionId == versionId && o.ProgramCourseId == courseId, ct)
            ?? throw new CurriculumNotFoundException("lớp học phần", offeringId);
        if (user.Role != SystemRoles.Admin && (user.Role != SystemRoles.Lecturer || offering.LecturerId != user.UserId)) throw new AcademicAccessException();
        return await db.ProgramCourses.SingleOrDefaultAsync(c => c.Id == courseId && c.ProgramVersionId == versionId && c.ArchivedAt == null, ct)
            ?? throw new CurriculumNotFoundException("học phần đang hoạt động", courseId);
    }
    public async Task<AssessmentScheme> SchemeAsync(long versionId, CancellationToken ct)
    {
        var v = await db.ProgramVersions.AsNoTracking().SingleOrDefaultAsync(v => v.Id == versionId, ct)
            ?? throw new CurriculumNotFoundException("phiên bản", versionId);
        var components = await Components(versionId, ct);
        var locked = await db.Set<GradeChange>().AnyAsync(g => db.Set<AssessmentComponent>().Any(c => c.Id == g.ComponentId && c.ProgramVersionId == versionId), ct);
        return new(versionId, v.GradeSchemeRevision, locked || v.ArchivedAt != null,
            components.Select(c => new AssessmentComponentInput(c.Code, c.Name, c.Weight, c.MaxScore)).ToArray());
    }
    private Task<AssessmentComponent[]> Components(long versionId, CancellationToken ct) => db.Set<AssessmentComponent>().AsNoTracking()
        .Where(c => c.ProgramVersionId == versionId).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).ToArrayAsync(ct);
    public async Task<AssessmentScheme> SaveSchemeAsync(long versionId, SaveAssessmentScheme request, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            var v = await writes.LockVersionAsync(versionId, ct);
            var scheme = await SchemeAsync(versionId, ct);
            if (scheme.Locked) throw new CurriculumConflictException("scheme_locked", "Cơ cấu điểm đã có lịch sử nhập điểm và được khóa. Hãy tạo phiên bản CTĐT mới nếu cần thay đổi.");
            Revision(scheme.Revision, request.Revision);
            await db.Set<AssessmentComponent>().Where(c => c.ProgramVersionId == versionId).ExecuteDeleteAsync(ct);
            db.AddRange(request.Components.Select((c, i) => new AssessmentComponent { ProgramVersionId = versionId,
                Code = c.Code, Name = c.Name.Trim(), Weight = c.Weight, MaxScore = c.MaxScore, DisplayOrder = i + 1 }));
            v.GradeSchemeRevision++; v.UpdatedAt = DateTime.UtcNow;
            return true;
        }, ct);
        return await SchemeAsync(versionId, ct);
    }
    private async Task<EnrollmentInfo[]> Infos(CourseEnrollment[] rows, long versionId, CancellationToken ct)
    {
        var ids = rows.Select(x => x.Id).ToArray();
        var components = await Components(versionId, ct);
        var grades = await db.Set<StudentGrade>().AsNoTracking().Where(g => ids.Contains(g.EnrollmentId)).ToArrayAsync(ct);
        return rows.Select(e => Info(e, components, grades.Where(g => g.EnrollmentId == e.Id).ToArray())).ToArray();
    }
    private static EnrollmentInfo Info(CourseEnrollment e, AssessmentComponent[] components, StudentGrade[] grades) =>
        new(e.Id, e.Student.StudentCode, e.Student.FullName, e.AcademicPeriod, e.Revision, grades.Length,
            GradeRules.Total(components.Select(c => ((decimal?)grades.SingleOrDefault(g => g.ComponentId == c.Id)?.Score, c.MaxScore, c.Weight))));
    public async Task<PagedResult<EnrollmentInfo>> StudentsAsync(long versionId, long courseId, long offeringId, string period, int page, string? search, CancellationToken ct)
    {
        var query = db.Set<CourseEnrollment>().AsNoTracking().Include(e => e.Student).Where(e => e.OfferingId == offeringId && e.ProgramCourseId == courseId && e.AcademicPeriod == period);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(e => EF.Functions.ILike(e.Student.StudentCode, "%" + search + "%") || EF.Functions.ILike(e.Student.FullName, "%" + search + "%"));
        var count = await query.LongCountAsync(ct);
        var rows = await query.OrderBy(e => e.Student.StudentCode).Skip((page - 1) * 50).Take(50).ToArrayAsync(ct);
        return PagedResult<EnrollmentInfo>.Create(await Infos(rows, versionId, ct), page, 50, count);
    }
    public async Task<EnrollmentInfo> EnrollAsync(long versionId, long courseId, long offeringId, EnrollStudent request, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            await LockCourse(versionId, courseId, offeringId, ct);
            var university = await UniversityId(versionId, ct);
            var student = await db.Set<AcademicStudent>().SingleOrDefaultAsync(s => s.UniversityId == university && s.StudentCode == request.StudentCode, ct);
            if (student is not null && student.FullName != request.FullName) throw new CurriculumConflictException("student_name_mismatch", "Mã sinh viên đã tồn tại với họ tên khác. Hãy kiểm tra danh sách gốc.");
            if (student is null) {
                student = new() { UniversityId = university, StudentCode = request.StudentCode, FullName = request.FullName, CreatedAt = DateTime.UtcNow };
                db.Add(student); await db.SaveChangesAsync(ct);
            }
            if (await db.Set<CourseEnrollment>().AnyAsync(e => e.StudentId == student.Id && e.OfferingId == offeringId && e.ProgramCourseId == courseId && e.AcademicPeriod == request.AcademicPeriod, ct))
                throw new CurriculumConflictException("already_enrolled", "Sinh viên đã có trong học phần và đợt học này.");
            var row = new CourseEnrollment { OfferingId = offeringId, ProgramCourseId = courseId, StudentId = student.Id, AcademicPeriod = request.AcademicPeriod, CreatedAt = DateTime.UtcNow };
            db.Add(row); await db.SaveChangesAsync(ct); return row.Id;
        }, ct);
        return (await GradesAsync(courseId, offeringId, id, ct)).Enrollment;
    }
    private Task<long> UniversityId(long versionId, CancellationToken ct) => db.ProgramVersions.Where(v => v.Id == versionId).Select(v => v.Program.UniversityId).SingleAsync(ct);
    public async Task<StudentGradebook> GradesAsync(long courseId, long offeringId, long enrollmentId, CancellationToken ct)
    {
        var enrollment = await db.Set<CourseEnrollment>().AsNoTracking().Include(e => e.Student).SingleOrDefaultAsync(e => e.Id == enrollmentId && e.OfferingId == offeringId && e.ProgramCourseId == courseId, ct)
            ?? throw new CurriculumNotFoundException("sinh viên trong học phần", enrollmentId);
        var versionId = await db.ProgramCourses.Where(c => c.Id == courseId).Select(c => c.ProgramVersionId).SingleAsync(ct);
        var components = await Components(versionId, ct);
        var grades = await db.Set<StudentGrade>().AsNoTracking().Where(g => g.EnrollmentId == enrollmentId).ToArrayAsync(ct);
        var history = await (from g in db.Set<GradeChange>().AsNoTracking() join c in db.Set<AssessmentComponent>() on g.ComponentId equals c.Id
            where g.EnrollmentId == enrollmentId orderby g.ChangedAt descending, g.Id descending
            select new GradeChangeInfo(c.Code, g.PreviousScore, g.NewScore, g.ActorId, g.ChangedAt, g.Source)).Take(50).ToArrayAsync(ct);
        return new(Info(enrollment, components, grades), await SchemeAsync(versionId, ct),
            components.ToDictionary(c => c.Code, c => (decimal?)grades.SingleOrDefault(g => g.ComponentId == c.Id)?.Score), history);
    }
    public async Task<StudentGradebook> SaveGradesAsync(long versionId, long courseId, long offeringId, long enrollmentId, SaveStudentGrades request, long actorId, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            await LockCourse(versionId, courseId, offeringId, ct);
            var scheme = await SchemeAsync(versionId, ct); Revision(scheme.Revision, request.SchemeRevision);
            AcademicValidator.Scores(request.Scores, scheme);
            var enrollment = await db.Set<CourseEnrollment>().SingleOrDefaultAsync(e => e.Id == enrollmentId && e.OfferingId == offeringId && e.ProgramCourseId == courseId, ct)
                ?? throw new CurriculumNotFoundException("sinh viên trong học phần", enrollmentId);
            Revision(enrollment.Revision, request.Revision);
            await PutGrades(enrollment, await Components(versionId, ct), request.Scores, actorId, "manual", ct);
            enrollment.Revision++; return true;
        }, ct);
        return await GradesAsync(courseId, offeringId, enrollmentId, ct);
    }
    private async Task PutGrades(CourseEnrollment enrollment, AssessmentComponent[] components, IReadOnlyDictionary<string, decimal?> scores, long actor, string source, CancellationToken ct)
    {
        foreach (var entry in scores)
        {
            var component = components.Single(c => c.Code == entry.Key);
            var current = await db.Set<StudentGrade>().FindAsync([enrollment.Id, component.Id], ct);
            var previous = current?.Score;
            if (previous == entry.Value) continue;
            db.Add(new GradeChange { EnrollmentId = enrollment.Id, ComponentId = component.Id, PreviousScore = previous, NewScore = entry.Value,
                ActorId = actor, ChangedAt = DateTime.UtcNow, Source = source });
            if (entry.Value is null) { if (current is not null) db.Remove(current); }
            else if (current is null) db.Add(new StudentGrade { EnrollmentId = enrollment.Id, ComponentId = component.Id, Score = entry.Value.Value, UpdatedBy = actor, UpdatedAt = DateTime.UtcNow });
            else { current.Score = entry.Value.Value; current.UpdatedBy = actor; current.UpdatedAt = DateTime.UtcNow; }
        }
    }
    public Task<AcademicImportPreview> ImportAsync(long versionId, long courseId, long offeringId, AcademicImportRequest request, bool apply, long actorId, CancellationToken ct) => writes.RunAsync(async () => {
        await LockCourse(versionId, courseId, offeringId, ct);
        var period = AcademicValidator.Period(request.AcademicPeriod);
        var scheme = await SchemeAsync(versionId, ct);
        if (request.Mode == "grades") Revision(scheme.Revision, request.SchemeRevision);
        var universityId = await UniversityId(versionId, ct);
        var checks = new List<AcademicImportRowResult>(); var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var raw in request.Rows)
        {
            var errors = new List<string>(); var row = raw; CourseEnrollment? enrollment = null;
            try {
                if (raw is null || raw.RowNumber < 1 || raw.Scores is null) AcademicValidator.Invalid("rows", "Dòng hoặc dữ liệu điểm không hợp lệ.");
                var normalized = AcademicValidator.Student(new(raw!.StudentCode, string.IsNullOrWhiteSpace(raw.FullName) && request.Mode == "grades" ? "Không cung cấp" : raw.FullName, period));
                row = raw with { StudentCode = normalized.StudentCode, FullName = request.Mode == "grades" && string.IsNullOrWhiteSpace(raw.FullName) ? "" : normalized.FullName };
                if (!seen.Add(row.StudentCode)) errors.Add("Mã sinh viên bị trùng trong tệp.");
                var student = await db.Set<AcademicStudent>().SingleOrDefaultAsync(s => s.UniversityId == universityId && s.StudentCode == row.StudentCode, ct);
                if (student is not null && row.FullName.Length > 0 && student.FullName != row.FullName) errors.Add("Họ tên không khớp mã sinh viên đã có.");
                if (student is not null) enrollment = await db.Set<CourseEnrollment>().SingleOrDefaultAsync(e => e.OfferingId == offeringId && e.ProgramCourseId == courseId && e.StudentId == student.Id && e.AcademicPeriod == period, ct);
                if (request.Mode == "students") {
                    if (row.Scores.Count > 0) errors.Add("Chế độ sinh viên không nhận cột điểm.");
                    if (apply && raw.ExpectedRevision != enrollment?.Revision) errors.Add("Danh sách đã thay đổi sau khi xem trước. Hãy xem trước lại.");
                }
                else {
                    AcademicValidator.Scores(row.Scores, scheme);
                    if (!row.Scores.Values.Any(s => s.HasValue)) errors.Add("Dòng không có điểm để nhập.");
                    if (enrollment is null) errors.Add("Sinh viên chưa có trong học phần/đợt học. Hãy import danh sách trước.");
                    else {
                        if (apply && raw.ExpectedRevision != enrollment.Revision) errors.Add("Điểm đã thay đổi sau khi xem trước. Hãy xem trước lại.");
                        var existing = await (from g in db.Set<StudentGrade>() join c in db.Set<AssessmentComponent>() on g.ComponentId equals c.Id
                            where g.EnrollmentId == enrollment.Id select new { c.Code, g.Score }).ToArrayAsync(ct);
                        if (!request.OverwriteExisting && existing.Any(g => row.Scores.TryGetValue(g.Code, out var value) && value is not null && value != g.Score))
                            errors.Add("Đã có điểm khác. Cần chọn cho phép cập nhật điểm đã có và xem trước lại.");
                    }
                }
            }
            catch (CurriculumValidationException ex) { errors.AddRange(ex.Errors.Values.SelectMany(x => x)); }
            checks.Add(new(row with { ExpectedRevision = enrollment?.Revision }, errors.Count > 0 ? "error" : request.Mode == "grades" ? "update" : enrollment is null ? "add" : "skip", errors));
        }
        var valid = checks.All(c => c.Errors.Count == 0);
        if (!apply || !valid) return new AcademicImportPreview(valid, checks);
        var components = await Components(versionId, ct); int applied = 0;
        foreach (var check in checks)
        {
            var row = check.Row;
            if (check.Action == "skip") continue;
            var student = await db.Set<AcademicStudent>().SingleOrDefaultAsync(s => s.UniversityId == universityId && s.StudentCode == row.StudentCode, ct);
            if (student is null) { student = new() { UniversityId = universityId, StudentCode = row.StudentCode, FullName = row.FullName, CreatedAt = DateTime.UtcNow }; db.Add(student); await db.SaveChangesAsync(ct); }
            if (request.Mode == "students") db.Add(new CourseEnrollment { OfferingId = offeringId, ProgramCourseId = courseId, StudentId = student.Id, AcademicPeriod = period, CreatedAt = DateTime.UtcNow });
            else {
                var enrollment = await db.Set<CourseEnrollment>().SingleAsync(e => e.OfferingId == offeringId && e.ProgramCourseId == courseId && e.StudentId == student.Id && e.AcademicPeriod == period, ct);
                await PutGrades(enrollment, components, row.Scores.Where(s => s.Value.HasValue).ToDictionary(s => s.Key, s => s.Value), actorId, "import", ct);
                enrollment.Revision++;
            }
            applied++;
        }
        return new AcademicImportPreview(true, checks, applied);
    }, ct);
    private static void Revision(int current, int supplied)
    {
        if (current != supplied) throw new CurriculumConflictException("stale_revision", "Dữ liệu đã thay đổi. Hãy tải lại trước khi lưu để tránh ghi đè thay đổi của người khác.");
    }
}
