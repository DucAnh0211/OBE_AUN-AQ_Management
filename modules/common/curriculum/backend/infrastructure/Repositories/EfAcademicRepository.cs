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
    public async Task<GradingPolicyInfo> PolicyForVersionAsync(long versionId, CancellationToken ct)
    {
        var (policy, bands) = await db.RulesAsync(await db.UniversityOfVersionAsync(versionId, ct), ct);
        return GradingQueries.Info(policy, bands);
    }

    // ---- Cơ cấu điểm trong đề cương học phần ----
    private Task<AssessmentComponent[]> Components(long courseId, CancellationToken ct) => db.Set<AssessmentComponent>().AsNoTracking()
        .Where(c => c.ProgramCourseId == courseId).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).ToArrayAsync(ct);
    public async Task<AssessmentScheme> SchemeAsync(long versionId, long courseId, CancellationToken ct)
    {
        var course = await db.ProgramCourses.AsNoTracking().Where(c => c.Id == courseId && c.ProgramVersionId == versionId)
            .Select(c => new { c.GradeSchemeRevision, Archived = c.ArchivedAt != null || c.ProgramVersion.ArchivedAt != null }).SingleOrDefaultAsync(ct)
            ?? throw new CurriculumNotFoundException("học phần", courseId);
        var components = await Components(courseId, ct);
        var ids = components.Select(c => c.Id).ToArray();
        var maps = await (from m in db.Set<AssessmentComponentClo>() join clo in db.Clos on m.CloId equals clo.Id
            where ids.Contains(m.ComponentId) select new { m.ComponentId, clo.Code, m.Weight }).ToArrayAsync(ct);
        var cloCodes = await db.Clos.AsNoTracking().Where(c => c.ProgramCourseId == courseId).OrderBy(c => c.Code).Select(c => c.Code).ToArrayAsync(ct);
        var locked = await db.Set<GradeChange>().AnyAsync(g => ids.Contains(g.ComponentId), ct);
        return new(courseId, course.GradeSchemeRevision, locked || course.Archived,
            components.Select(c => new AssessmentComponentInput(c.Code, c.Name, c.Weight, c.MaxScore, c.Kind,
                maps.Where(m => m.ComponentId == c.Id).ToDictionary(m => m.Code, m => m.Weight))).ToArray(), cloCodes);
    }
    public async Task<AssessmentScheme> SaveSchemeAsync(long versionId, long courseId, SaveAssessmentScheme request, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            await writes.LockVersionAsync(versionId, ct);
            var course = await db.ProgramCourses.SingleOrDefaultAsync(c => c.Id == courseId && c.ProgramVersionId == versionId && c.ArchivedAt == null, ct)
                ?? throw new CurriculumNotFoundException("học phần đang hoạt động", courseId);
            var scheme = await SchemeAsync(versionId, courseId, ct);
            Revision(scheme.Revision, request.Revision);
            var sameStructure = scheme.Components.Count == request.Components.Count && scheme.Components.Zip(request.Components)
                .All(p => p.First.Code == p.Second.Code && p.First.Weight == p.Second.Weight && p.First.MaxScore == p.Second.MaxScore && p.First.Kind == p.Second.Kind);
            if (scheme.Locked && !sameStructure)
                throw new CurriculumConflictException("scheme_locked", "Cơ cấu điểm đã có lịch sử nhập điểm nên chỉ được sửa tên đầu điểm và liên kết CLO. Thay đổi trọng số cần phiên bản CTĐT mới.");
            var clos = await db.Clos.Where(c => c.ProgramCourseId == courseId).ToDictionaryAsync(c => c.Code, c => c.Id, ct);
            var existing = await db.Set<AssessmentComponent>().Where(c => c.ProgramCourseId == courseId).ToListAsync(ct);
            var existingIds = existing.Select(c => c.Id).ToArray();
            await db.Set<AssessmentComponentClo>().Where(m => existingIds.Contains(m.ComponentId)).ExecuteDeleteAsync(ct);
            if (sameStructure)
            {
                foreach (var component in existing) component.Name = request.Components.Single(c => c.Code == component.Code).Name.Trim();
                db.AddRange(existing.SelectMany(c => Maps(c.Id, request.Components.Single(x => x.Code == c.Code), clos)));
            }
            else
            {
                await db.Set<AssessmentComponent>().Where(c => c.ProgramCourseId == courseId).ExecuteDeleteAsync(ct);
                var added = request.Components.Select((c, i) => (Input: c, Entity: new AssessmentComponent { ProgramCourseId = courseId, Code = c.Code,
                    Name = c.Name.Trim(), Kind = c.Kind, Weight = c.Weight, MaxScore = c.MaxScore, DisplayOrder = i + 1 })).ToArray();
                db.AddRange(added.Select(a => a.Entity)); await db.SaveChangesAsync(ct);
                db.AddRange(added.SelectMany(a => Maps(a.Entity.Id, a.Input, clos)));
            }
            course.GradeSchemeRevision++; course.UpdatedAt = DateTime.UtcNow;
            return true;
        }, ct);
        return await SchemeAsync(versionId, courseId, ct);
    }
    private static IEnumerable<AssessmentComponentClo> Maps(long componentId, AssessmentComponentInput input, IReadOnlyDictionary<string, long> clos) =>
        (input.Clos ?? new Dictionary<string, decimal>()).Select(m => new AssessmentComponentClo { ComponentId = componentId, CloId = clos[m.Key], Weight = m.Value });
    public Task<SchemeTemplateResult> ApplyTemplateAsync(long versionId, long sourceCourseId, CancellationToken ct) => writes.RunAsync(async () => {
        await writes.LockVersionAsync(versionId, ct);
        var source = await Components(sourceCourseId, ct);
        if (source.Length == 0) throw new CurriculumConflictException("template_empty", "Học phần mẫu chưa có cơ cấu điểm.");
        var courses = await db.ProgramCourses.Where(c => c.ProgramVersionId == versionId && c.ArchivedAt == null && c.Id != sourceCourseId).ToListAsync(ct);
        var courseIds = courses.Select(c => c.Id).ToArray();
        var configured = (await db.Set<AssessmentComponent>().Where(c => courseIds.Contains(c.ProgramCourseId)).Select(c => c.ProgramCourseId).Distinct().ToArrayAsync(ct)).ToHashSet();
        var updated = 0;
        foreach (var course in courses.Where(c => !configured.Contains(c.Id)))
        {
            db.AddRange(source.Select(c => new AssessmentComponent { ProgramCourseId = course.Id, Code = c.Code, Name = c.Name, Kind = c.Kind,
                Weight = c.Weight, MaxScore = c.MaxScore, DisplayOrder = c.DisplayOrder }));
            course.GradeSchemeRevision++; course.UpdatedAt = DateTime.UtcNow; updated++;
        }
        return new SchemeTemplateResult(updated, courses.Count - updated);
    }, ct);
    public async Task<IReadOnlyList<AssessmentCoverageItem>> CoverageAsync(long versionId, CancellationToken ct)
    {
        var courses = await db.ProgramCourses.AsNoTracking().Where(c => c.ProgramVersionId == versionId && c.ArchivedAt == null)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).Select(c => new { c.Id, c.CourseCodeSnapshot, c.CourseNameSnapshot }).ToArrayAsync(ct);
        var ids = courses.Select(c => c.Id).ToArray();
        var components = await db.Set<AssessmentComponent>().AsNoTracking().Where(c => ids.Contains(c.ProgramCourseId)).Select(c => new { c.Id, c.ProgramCourseId, c.Code, c.DisplayOrder }).ToArrayAsync(ct);
        var componentIds = components.Select(c => c.Id).ToArray();
        var maps = await db.Set<AssessmentComponentClo>().AsNoTracking().Where(m => componentIds.Contains(m.ComponentId)).ToArrayAsync(ct);
        var clos = await db.Clos.AsNoTracking().Where(c => ids.Contains(c.ProgramCourseId)).Select(c => new { c.Id, c.ProgramCourseId, c.Code }).ToArrayAsync(ct);
        return courses.Select(c => new AssessmentCoverageItem(c.Id, c.CourseCodeSnapshot, c.CourseNameSnapshot, components.Count(x => x.ProgramCourseId == c.Id),
            components.Where(x => x.ProgramCourseId == c.Id && !maps.Any(m => m.ComponentId == x.Id)).OrderBy(x => x.DisplayOrder).Select(x => x.Code).ToArray(),
            clos.Where(x => x.ProgramCourseId == c.Id && !maps.Any(m => m.CloId == x.Id)).OrderBy(x => x.Code).Select(x => x.Code).ToArray())).ToArray();
    }

    // ---- Sổ điểm lớp học phần ----
    private async Task<CourseOffering> LockOffering(CourseOfferingInfo info, CancellationToken ct)
    {
        await writes.LockVersionAsync(info.VersionId, ct);
        var offering = await db.Set<CourseOffering>().Include(o => o.Lecturers).SingleOrDefaultAsync(o => o.Id == info.Id && o.ProgramVersionId == info.VersionId, ct)
            ?? throw new CurriculumNotFoundException("lớp học phần", info.Id);
        if (user.Role != SystemRoles.Admin && (user.Role != SystemRoles.Lecturer || offering.Lecturers.All(l => l.UserId != user.UserId))) throw new AcademicAccessException();
        if (!await db.ProgramCourses.AnyAsync(c => c.Id == offering.ProgramCourseId && c.ArchivedAt == null, ct))
            throw new CurriculumNotFoundException("học phần đang hoạt động", offering.ProgramCourseId);
        return offering;
    }
    private static void RequireDraft(CourseOffering offering)
    {
        if (offering.GradeStatus != AcademicCodes.Draft) throw new CurriculumConflictException("grades_not_draft", "Sổ điểm đã nộp hoặc đã công bố, không thể thêm sinh viên hay import.");
    }
    private sealed record Rules(GradingPolicy Policy, GradeBand[] Bands);
    private async Task<Rules> RulesForVersion(long versionId, CancellationToken ct)
    {
        var (policy, bands) = await db.RulesAsync(await db.UniversityOfVersionAsync(versionId, ct), ct);
        return new(policy, bands);
    }
    private static decimal? Total(AssessmentComponent[] components, IEnumerable<StudentGrade> grades, Rules rules) =>
        GradeRules.Total(components.Select(c => ((decimal?)grades.SingleOrDefault(g => g.ComponentId == c.Id)?.Score, c.MaxScore, c.Weight)), rules.Policy.CourseScoreDecimals);
    private static EnrollmentInfo Info(CourseEnrollment e, AssessmentComponent[] components, StudentGrade[] grades, Rules rules, string? homeClass)
    {
        var total = Total(components, grades, rules);
        var outcome = e.PublishedAt != null ? new GradeOutcome(e.FinalScore, e.LetterGrade, e.GradePoint, e.Passed) : GradeRules.Outcome(total, e.SpecialMark, rules.Bands);
        return new(e.Id, e.Student.StudentCode, e.Student.FullName, e.AcademicPeriod, e.Revision, grades.Length,
            e.PublishedAt != null ? e.FinalScore : total, outcome.Letter, outcome.Point4, outcome.Passed, e.SpecialMark, e.AttemptKind, homeClass, e.PublishedAt != null);
    }
    private async Task<EnrollmentInfo[]> Infos(CourseEnrollment[] rows, CourseOfferingInfo offering, CancellationToken ct)
    {
        var ids = rows.Select(x => x.Id).ToArray();
        var components = await Components(offering.ProgramCourseId, ct);
        var grades = await db.Set<StudentGrade>().AsNoTracking().Where(g => ids.Contains(g.EnrollmentId)).ToArrayAsync(ct);
        var rules = await RulesForVersion(offering.VersionId, ct);
        var classIds = rows.Where(r => r.Student.HomeClassId != null).Select(r => r.Student.HomeClassId!.Value).Distinct().ToArray();
        var classes = await db.Set<HomeClass>().AsNoTracking().Where(c => classIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Code, ct);
        return rows.Select(e => Info(e, components, grades.Where(g => g.EnrollmentId == e.Id).ToArray(), rules,
            e.Student.HomeClassId is long h && classes.TryGetValue(h, out var code) ? code : null)).ToArray();
    }
    public async Task<PagedResult<EnrollmentInfo>> StudentsAsync(CourseOfferingInfo offering, int page, string? search, CancellationToken ct)
    {
        var query = db.Set<CourseEnrollment>().AsNoTracking().Include(e => e.Student).Where(e => e.OfferingId == offering.Id);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(e => EF.Functions.ILike(e.Student.StudentCode, "%" + search + "%") || EF.Functions.ILike(e.Student.FullName, "%" + search + "%"));
        var count = await query.LongCountAsync(ct);
        var rows = await query.OrderBy(e => e.Student.StudentCode).Skip((page - 1) * 50).Take(50).ToArrayAsync(ct);
        return PagedResult<EnrollmentInfo>.Create(await Infos(rows, offering, ct), page, 50, count);
    }
    public async Task<EnrollmentInfo> EnrollAsync(CourseOfferingInfo info, EnrollStudent request, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            var offering = await LockOffering(info, ct); RequireDraft(offering);
            var university = await db.UniversityOfVersionAsync(offering.ProgramVersionId, ct);
            var student = await db.Set<AcademicStudent>().SingleOrDefaultAsync(s => s.UniversityId == university && s.StudentCode == request.StudentCode, ct);
            if (student is not null && student.FullName != request.FullName) throw new CurriculumConflictException("student_name_mismatch", "Mã sinh viên đã tồn tại với họ tên khác. Hãy kiểm tra danh sách gốc.");
            if (student is null) {
                student = new() { UniversityId = university, StudentCode = request.StudentCode, FullName = request.FullName, CreatedAt = DateTime.UtcNow };
                db.Add(student); await db.SaveChangesAsync(ct);
            }
            if (await db.Set<CourseEnrollment>().AnyAsync(e => e.StudentId == student.Id && e.OfferingId == offering.Id, ct))
                throw new CurriculumConflictException("already_enrolled", "Sinh viên đã có trong lớp học phần này.");
            if (await EnrollmentWriter.BlockerAsync(db, offering, student, ct) is string blocker) throw new CurriculumConflictException("enroll_blocked", blocker);
            if (await EnrollmentWriter.OverCapacityAsync(db, offering, 1, ct)) throw new CurriculumConflictException("offering_full", $"Lớp học phần đã đủ sĩ số tối đa {offering.Capacity}.");
            var row = await EnrollmentWriter.AddAsync(db, offering, student, ct);
            await db.SaveChangesAsync(ct); return row.Id;
        }, ct);
        return (await GradesAsync(info, id, ct)).Enrollment;
    }
    public async Task<StudentGradebook> GradesAsync(CourseOfferingInfo offering, long enrollmentId, CancellationToken ct)
    {
        var enrollment = await db.Set<CourseEnrollment>().AsNoTracking().Include(e => e.Student).SingleOrDefaultAsync(e => e.Id == enrollmentId && e.OfferingId == offering.Id, ct)
            ?? throw new CurriculumNotFoundException("sinh viên trong lớp học phần", enrollmentId);
        var components = await Components(offering.ProgramCourseId, ct);
        var history = await (from g in db.Set<GradeChange>().AsNoTracking() join c in db.Set<AssessmentComponent>() on g.ComponentId equals c.Id
            where g.EnrollmentId == enrollmentId orderby g.ChangedAt descending, g.Id descending
            select new GradeChangeInfo(c.Code, g.PreviousScore, g.NewScore, g.ActorId, g.ChangedAt, g.Source, g.Reason)).Take(50).ToArrayAsync(ct);
        var grades = await db.Set<StudentGrade>().AsNoTracking().Where(g => g.EnrollmentId == enrollmentId).ToArrayAsync(ct);
        var info = (await Infos([enrollment], offering, ct))[0];
        return new(info, await SchemeAsync(offering.VersionId, offering.ProgramCourseId, ct),
            components.ToDictionary(c => c.Code, c => (decimal?)grades.SingleOrDefault(g => g.ComponentId == c.Id)?.Score), history);
    }
    public async Task<StudentGradebook> SaveGradesAsync(CourseOfferingInfo info, long enrollmentId, SaveStudentGrades request, long actorId, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            var offering = await LockOffering(info, ct);
            if (offering.GradeStatus != AcademicCodes.Draft && user.Role != SystemRoles.Admin)
                throw new CurriculumConflictException("grades_locked", "Điểm đã nộp hoặc đã công bố. Liên hệ phòng đào tạo để trả lại hoặc phúc khảo.");
            var published = offering.GradeStatus == AcademicCodes.Published;
            if (published && request.Reason is null) AcademicValidator.Invalid("reason", "Sửa điểm đã công bố cần ghi lý do (ví dụ kết quả phúc khảo).");
            var scheme = await SchemeAsync(offering.ProgramVersionId, offering.ProgramCourseId, ct); Revision(scheme.Revision, request.SchemeRevision);
            AcademicValidator.Scores(request.Scores, scheme, allowEmpty: true);
            var enrollment = await db.Set<CourseEnrollment>().SingleOrDefaultAsync(e => e.Id == enrollmentId && e.OfferingId == offering.Id, ct)
                ?? throw new CurriculumNotFoundException("sinh viên trong lớp học phần", enrollmentId);
            Revision(enrollment.Revision, request.Revision);
            var components = await Components(offering.ProgramCourseId, ct);
            await PutGrades(enrollment, components, request.Scores, actorId, published ? "regrade" : "manual", request.Reason, ct);
            enrollment.SpecialMark = request.SpecialMark;
            if (published)
            {
                await db.SaveChangesAsync(ct);
                Snapshot(enrollment, components, await db.Set<StudentGrade>().Where(g => g.EnrollmentId == enrollment.Id).ToArrayAsync(ct), await RulesForVersion(offering.ProgramVersionId, ct));
            }
            enrollment.Revision++; return true;
        }, ct);
        return await GradesAsync(info, enrollmentId, ct);
    }
    private async Task PutGrades(CourseEnrollment enrollment, AssessmentComponent[] components, IReadOnlyDictionary<string, decimal?> scores, long actor, string source, string? reason, CancellationToken ct)
    {
        foreach (var entry in scores)
        {
            var component = components.Single(c => c.Code == entry.Key);
            var current = await db.Set<StudentGrade>().FindAsync([enrollment.Id, component.Id], ct);
            var previous = current?.Score;
            if (previous == entry.Value) continue;
            db.Add(new GradeChange { EnrollmentId = enrollment.Id, ComponentId = component.Id, PreviousScore = previous, NewScore = entry.Value,
                ActorId = actor, ChangedAt = DateTime.UtcNow, Source = source, Reason = reason });
            if (entry.Value is null) { if (current is not null) db.Remove(current); }
            else if (current is null) db.Add(new StudentGrade { EnrollmentId = enrollment.Id, ComponentId = component.Id, Score = entry.Value.Value, UpdatedBy = actor, UpdatedAt = DateTime.UtcNow });
            else { current.Score = entry.Value.Value; current.UpdatedBy = actor; current.UpdatedAt = DateTime.UtcNow; }
        }
    }
    /// <summary>Lưu kết quả công bố; không phụ thuộc thang điểm thay đổi sau này.</summary>
    private static void Snapshot(CourseEnrollment e, AssessmentComponent[] components, IEnumerable<StudentGrade> grades, Rules rules)
    {
        var outcome = GradeRules.Outcome(Total(components, grades, rules), e.SpecialMark, rules.Bands);
        e.FinalScore = outcome.FinalScore; e.LetterGrade = outcome.Letter; e.GradePoint = outcome.Point4; e.Passed = outcome.Passed; e.PublishedAt = DateTime.UtcNow;
    }
    private static void ClearSnapshot(CourseEnrollment e) { e.FinalScore = null; e.LetterGrade = null; e.GradePoint = null; e.Passed = null; e.PublishedAt = null; }
    public Task ChangeGradeStatusAsync(CourseOfferingInfo info, string action, string? reason, long actorId, CancellationToken ct) => writes.RunAsync(async () => {
        var offering = await LockOffering(info, ct);
        var enrollments = await db.Set<CourseEnrollment>().Include(e => e.Student).Where(e => e.OfferingId == offering.Id).ToListAsync(ct);
        var components = await Components(offering.ProgramCourseId, ct);
        var ids = enrollments.Select(e => e.Id).ToArray();
        var grades = await db.Set<StudentGrade>().AsNoTracking().Where(g => ids.Contains(g.EnrollmentId)).ToArrayAsync(ct);
        string[] Incomplete() => enrollments.Where(e => e.SpecialMark is null && (components.Length == 0 || grades.Count(g => g.EnrollmentId == e.Id) < components.Length))
            .Select(e => e.Student.StudentCode).OrderBy(x => x).ToArray();
        void RequireComplete()
        {
            if (enrollments.Count == 0) throw new CurriculumConflictException("offering_empty", "Lớp học phần chưa có sinh viên.");
            var missing = Incomplete();
            if (missing.Length > 0) throw new CurriculumConflictException("grades_incomplete",
                $"Còn {missing.Length} sinh viên chưa đủ điểm hoặc chưa có điểm đặc biệt: {string.Join(", ", missing.Take(10))}{(missing.Length > 10 ? "…" : "")}.");
        }
        var now = DateTime.UtcNow;
        switch (action)
        {
            case "submit":
                if (offering.GradeStatus != AcademicCodes.Draft) throw new CurriculumConflictException("grade_status", "Chỉ nộp sổ điểm đang ở trạng thái đang nhập.");
                RequireComplete();
                offering.GradeStatus = AcademicCodes.Submitted; offering.SubmittedAt = now; offering.SubmittedBy = actorId; offering.StatusNote = null;
                break;
            case "publish":
                if (offering.GradeStatus != AcademicCodes.Submitted) throw new CurriculumConflictException("grade_status", "Chỉ công bố sổ điểm đã được giảng viên nộp.");
                RequireComplete();
                var rules = await RulesForVersion(offering.ProgramVersionId, ct);
                foreach (var e in enrollments) Snapshot(e, components, grades.Where(g => g.EnrollmentId == e.Id), rules);
                offering.GradeStatus = AcademicCodes.Published; offering.PublishedAt = now; offering.PublishedBy = actorId;
                break;
            case "reopen":
                if (offering.GradeStatus == AcademicCodes.Draft) throw new CurriculumConflictException("grade_status", "Sổ điểm đang ở trạng thái đang nhập.");
                foreach (var e in enrollments) ClearSnapshot(e);
                offering.GradeStatus = AcademicCodes.Draft; offering.StatusNote = reason;
                offering.SubmittedAt = null; offering.SubmittedBy = null; offering.PublishedAt = null; offering.PublishedBy = null;
                break;
        }
        return true;
    }, ct);
    public Task<AcademicImportPreview> ImportAsync(CourseOfferingInfo info, AcademicImportRequest request, bool apply, long actorId, CancellationToken ct) => writes.RunAsync(async () => {
        var offering = await LockOffering(info, ct); RequireDraft(offering);
        var period = AcademicValidator.Period(request.AcademicPeriod);
        var scheme = await SchemeAsync(offering.ProgramVersionId, offering.ProgramCourseId, ct);
        if (request.Mode == "grades") Revision(scheme.Revision, request.SchemeRevision);
        var universityId = await db.UniversityOfVersionAsync(offering.ProgramVersionId, ct);
        var checks = new List<AcademicImportRowResult>(); var seen = new HashSet<string>(StringComparer.Ordinal);
        var adding = 0;
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
                if (student is not null) enrollment = await db.Set<CourseEnrollment>().SingleOrDefaultAsync(e => e.OfferingId == offering.Id && e.StudentId == student.Id, ct);
                if (request.Mode == "students") {
                    if (row.Scores.Count > 0) errors.Add("Chế độ sinh viên không nhận cột điểm.");
                    if (apply && raw.ExpectedRevision != enrollment?.Revision) errors.Add("Danh sách đã thay đổi sau khi xem trước. Hãy xem trước lại.");
                    if (enrollment is null && student is not null && await EnrollmentWriter.BlockerAsync(db, offering, student, ct) is string blocker) errors.Add(blocker);
                    if (enrollment is null) adding++;
                }
                else {
                    AcademicValidator.Scores(row.Scores, scheme);
                    if (!row.Scores.Values.Any(s => s.HasValue)) errors.Add("Dòng không có điểm để nhập.");
                    if (enrollment is null) errors.Add("Sinh viên chưa có trong lớp học phần. Hãy import danh sách trước.");
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
        if (request.Mode == "students" && await EnrollmentWriter.OverCapacityAsync(db, offering, adding, ct))
            checks.Add(new(new(0, "", "", new Dictionary<string, decimal?>()), "error", [$"Vượt sĩ số tối đa {offering.Capacity} của lớp học phần."]));
        var valid = checks.All(c => c.Errors.Count == 0);
        if (!apply || !valid) return new AcademicImportPreview(valid, checks);
        var components = await Components(offering.ProgramCourseId, ct); int applied = 0;
        foreach (var check in checks)
        {
            var row = check.Row;
            if (check.Action == "skip") continue;
            var student = await db.Set<AcademicStudent>().SingleOrDefaultAsync(s => s.UniversityId == universityId && s.StudentCode == row.StudentCode, ct);
            if (student is null) { student = new() { UniversityId = universityId, StudentCode = row.StudentCode, FullName = row.FullName, CreatedAt = DateTime.UtcNow }; db.Add(student); await db.SaveChangesAsync(ct); }
            if (request.Mode == "students") await EnrollmentWriter.AddAsync(db, offering, student, ct);
            else {
                var enrollment = await db.Set<CourseEnrollment>().SingleAsync(e => e.OfferingId == offering.Id && e.StudentId == student.Id, ct);
                await PutGrades(enrollment, components, row.Scores.Where(s => s.Value.HasValue).ToDictionary(s => s.Key, s => s.Value), actorId, "import", null, ct);
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
