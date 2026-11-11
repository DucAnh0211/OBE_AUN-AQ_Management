using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
using CurriculumProgram = ObeAunQa.Modules.Curriculum.Domain.Entities.Program;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

// Explicit demo initialization only. Never resets or merges a user's existing curriculum.
public sealed class CurriculumDemoSeeder(CurriculumDbContext db)
{
    public async Task<bool> SeedIfEmptyAsync(CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (await db.Programs.AnyAsync(ct)) return false;
        if (await db.Courses.AnyAsync(ct) || await db.SourceDocuments.AnyAsync(ct))
            throw new InvalidOperationException("Demo seeding requires an empty curriculum database.");

        using var v1 = Read("V1.json");
        using var v2 = Read("V2.json");
        using var m1 = Read("V1.manifest.json");
        using var m2 = Read("V2.manifest.json");
        var first = v1.RootElement;
        var metadata = first.GetProperty("metadata");
        var workbook = new SourceDocument { FileName = Text(metadata, "source_workbook")!, FileKind = "xlsx", Sha256 = Text(m1.RootElement, "workbookSha256")! };
        var syllabus = new SourceDocument { FileName = Text(metadata, "source_course_syllabus")!, FileKind = "docx", Sha256 = Text(m1.RootElement, "syllabusSha256")! };
        var university = new University { Code = "TRUONG-01", Name = "Trường chưa đặt tên" };
        db.Universities.Add(university);
        await db.SaveChangesAsync(ct);
        var program = new CurriculumProgram { UniversityId = university.Id, Code = "CS", Name = "Khoa học máy tính" };
        db.Programs.Add(program);
        db.SourceDocuments.AddRange(workbook, syllabus);
        await db.SaveChangesAsync(ct);

        var baseline = await AddVersionAsync(program, first, m1.RootElement, workbook, syllabus, null, ct);
        var current = await AddVersionAsync(program, v2.RootElement, m2.RootElement, workbook, syllabus, baseline, ct);
        await SeedAcademicsAsync(university, program, current, ct);
        var verifier = new CurriculumDemoVerifier(db);
        await verifier.VerifyAsync(ct);
        await verifier.VerifyAcademicsAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    private async Task<ProgramVersion> AddVersionAsync(CurriculumProgram program, JsonElement data, JsonElement manifest,
        SourceDocument workbook, SourceDocument syllabus, ProgramVersion? source, CancellationToken ct)
    {
        var isBaseline = source == null;
        var version = new ProgramVersion { ProgramId = program.Id, VersionCode = Text(data.GetProperty("metadata"), "program_version")!,
            DatasetStatus = isBaseline ? "finalized" : "draft", IsCurrent = isBaseline,
            PublishedAt = isBaseline ? DateTime.UtcNow : null, SourceVersionId = source?.Id };
        db.ProgramVersions.Add(version);
        await db.SaveChangesAsync(ct);
        var plos = data.GetProperty("program_plos_original").EnumerateArray().Select(p => new Plo {
            ProgramVersionId = version.Id, Code = Text(p, "id")!, Statement = Text(p, "text")!,
            LevelCode = Text(p, "level"), SourceCell = Text(p, "source_cell"),
            Provenance = isBaseline ? "original_curriculum_map" : "copied_official_plo_from_v1"
        }).ToDictionary(p => p.Code);
        db.Plos.AddRange(plos.Values);

        var originalCourses = isBaseline ? new Dictionary<int, ProgramCourse>() :
            await db.ProgramCourses.Include(c => c.Course).Where(c => c.ProgramVersionId == source!.Id).ToDictionaryAsync(c => c.SourceRow!.Value, ct);
        var courses = new Dictionary<int, ProgramCourse>();
        var sourceRows = data.GetProperty("courses_finalized").EnumerateArray().OrderBy(c => c.GetProperty("source_row").GetInt32()).ToArray();
        foreach (var item in sourceRows)
        {
            var row = item.GetProperty("source_row").GetInt32();
            var original = isBaseline ? null : originalCourses[row];
            var flags = item.GetProperty("quality_flags");
            var code = Text(item, "course_code_source_value")?.Trim();
            var course = new ProgramCourse {
                ProgramVersionId = version.Id, SourceRow = row,
                Course = original?.Course ?? new Course { UniversityId = program.UniversityId, Name = Text(item, "name")!, InstitutionalCode = (courses.Count + 1).ToString("D6"), LegacyInstitutionalCode = code },
                Credits = original?.Credits ?? item.GetProperty("credits").GetInt32(),
                CourseCodeSourceValue = original?.CourseCodeSourceValue ?? Text(item, "course_code_source_value"),
                SemesterSourceValue = original?.SemesterSourceValue ?? Text(item, "semester_source_value"),
                DatasetStatus = isBaseline ? Text(item, "dataset_status")! : "draft",
                QualityFlags = original?.QualityFlags ?? flags.GetRawText(),
                CourseNameSnapshot = original?.CourseNameSnapshot ?? Text(item, "name")!,
                CourseCodeSnapshot = original?.CourseCodeSnapshot ?? (courses.Count + 1).ToString("D6"),
                LegacyCourseCodeSnapshot = original?.LegacyCourseCodeSnapshot ?? code,
                DisplayOrder = original?.DisplayOrder ?? courses.Count + 1
            };
            courses.Add(row, course);
            db.ProgramCourses.Add(course);
        }
        await db.SaveChangesAsync(ct);

        foreach (var item in sourceRows)
        {
            var course = courses[item.GetProperty("source_row").GetInt32()];
            var mappings = item.GetProperty("plo_links").EnumerateArray().Select(link => new CoursePlo {
                ProgramVersionId = version.Id, ProgramCourseId = course.Id, PloId = plos[Text(link, "plo_id")!].Id,
                WeightCode = Text(link, "weight_code")![0], ProgressionCode = Text(link, "progression_code")![0],
                Provenance = Text(link, "provenance")!, Fit = Text(link, "fit"), FitReason = Text(link, "fit_reason")
            }).ToDictionary(m => plos.Values.Single(p => p.Id == m.PloId).Code);
            db.CoursePlos.AddRange(mappings.Values);
            await db.SaveChangesAsync(ct);
            foreach (var outcome in item.GetProperty("clos").EnumerateArray())
            {
                var clo = new Clo { ProgramCourseId = course.Id, Code = Text(outcome, "id")!, Statement = Text(outcome, "text")!,
                    LevelCode = Text(outcome, "bloom_level"), Provenance = Text(outcome, "provenance")!,
                    SourceReference = outcome.TryGetProperty("source_reference", out var reference) ? reference.GetRawText() : null,
                    AssumptionNote = Text(outcome, "assumption_note"), DatasetStatus = isBaseline ? Text(outcome, "dataset_status")! : "draft" };
                db.Clos.Add(clo);
                await db.SaveChangesAsync(ct);
                foreach (var support in outcome.GetProperty("supports_plo").EnumerateArray())
                    db.CloPlos.Add(new CloPlo { CloId = clo.Id, ProgramCourseId = course.Id, CoursePloId = mappings[support.GetString()!].Id });
            }
        }
        var document = new SourceDocument { FileName = Text(manifest, "datasetFileName")!, FileKind = "json", Sha256 = Text(manifest, "datasetSha256")! };
        var batch = new ImportBatch { ProgramVersionId = version.Id, DatasetDocument = document, WorkbookDocument = workbook,
            SyllabusDocument = syllabus, DatasetSha256 = document.Sha256 };
        foreach (var raw in data.GetProperty("curriculum_map_source_data").EnumerateArray())
            batch.SourceCourseRows.Add(new SourceCourseRow { SourceRow = raw.GetProperty("source_row").GetInt32(), RawData = raw.GetRawText() });
        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync(ct);
        return version;
    }

    /// <summary>
    /// Dữ liệu học vụ mẫu (giả lập, không phải dữ liệu thật): khoa/bộ môn, quy chế TT08, một lớp sinh hoạt,
    /// cơ cấu điểm có liên kết CLO cho 3 học phần, lớp học phần đã công bố/đang nhập và một ca học lại.
    /// </summary>
    private async Task SeedAcademicsAsync(University university, CurriculumProgram program, ProgramVersion version, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var policy = new GradingPolicy { UniversityId = university.Id, UpdatedAt = now };
        db.Add(policy);
        db.AddRange(Domain.GradeRules.DefaultBands.Select((b, i) => new GradeScaleBand { UniversityId = university.Id, Letter = b.Letter, MinScore = b.MinScore,
            Point4 = b.Point4, IsPassing = b.IsPassing, DisplayOrder = i + 1 }));
        var faculty = new OrgUnit { UniversityId = university.Id, Kind = Domain.AcademicCodes.Faculty, Code = "CNTT", Name = "Khoa Công nghệ thông tin", CreatedAt = now };
        db.Add(faculty); await db.SaveChangesAsync(ct);
        var department = new OrgUnit { UniversityId = university.Id, ParentId = faculty.Id, Kind = Domain.AcademicCodes.Department, Code = "KHMT", Name = "Bộ môn Khoa học máy tính", CreatedAt = now };
        db.Add(department); await db.SaveChangesAsync(ct);
        program.FacultyId = faculty.Id;
        await db.Courses.Where(c => c.UniversityId == university.Id).ExecuteUpdateAsync(s => s.SetProperty(c => c.DepartmentId, department.Id), ct);

        var homeClass = new HomeClass { ProgramVersionId = version.Id, CohortCode = "K66", Code = "KHMT01-K66", Name = "Khoa học máy tính 01 – K66", CreatedAt = now };
        db.Add(homeClass); await db.SaveChangesAsync(ct);
        var students = new[] { "Nguyễn Văn An", "Trần Thị Bình", "Lê Minh Châu", "Phạm Quốc Dũng" }.Select((name, i) => new AcademicStudent { UniversityId = university.Id,
            StudentCode = $"DEMO{i + 1:000}", FullName = name, HomeClassId = homeClass.Id, ProgramVersionId = version.Id, CohortCode = "K66", CreatedAt = now }).ToArray();
        db.AddRange(students); await db.SaveChangesAsync(ct);

        var courses = await db.ProgramCourses.Where(c => c.ProgramVersionId == version.Id && c.Credits >= 2).OrderBy(c => c.DisplayOrder).Take(3).ToListAsync(ct);
        var schemes = new Dictionary<long, AssessmentComponent[]>();
        foreach (var course in courses)
        {
            var parts = new[] { new AssessmentComponent { ProgramCourseId = course.Id, Code = "QT", Name = "Quá trình", Kind = "process", Weight = 40, MaxScore = 10, DisplayOrder = 1 },
                new AssessmentComponent { ProgramCourseId = course.Id, Code = "THI", Name = "Thi cuối kỳ", Kind = Domain.AcademicCodes.FinalExam, Weight = 60, MaxScore = 10, DisplayOrder = 2 } };
            db.AddRange(parts); await db.SaveChangesAsync(ct);
            var clos = await db.Clos.Where(c => c.ProgramCourseId == course.Id).OrderBy(c => c.Code).Select(c => c.Id).ToArrayAsync(ct);
            if (clos.Length > 0) db.Add(new AssessmentComponentClo { ComponentId = parts[0].Id, CloId = clos[0], Weight = 100 });
            db.AddRange(clos.Select(id => new AssessmentComponentClo { ComponentId = parts[1].Id, CloId = id, Weight = 100 }));
            course.GradeSchemeRevision = 1; schemes[course.Id] = parts;
        }
        await db.SaveChangesAsync(ct);

        var bands = Domain.GradeRules.DefaultBands;
        async Task Offering(ProgramCourse course, string period, bool publish, (AcademicStudent Student, decimal Qt, decimal Thi, string Attempt)[] rows)
        {
            var offering = new CourseOffering { ProgramVersionId = version.Id, ProgramCourseId = course.Id, AcademicPeriod = period, GroupName = "N01", TargetHomeClassId = homeClass.Id,
                Capacity = 60, GradeStatus = publish ? Domain.AcademicCodes.Published : Domain.AcademicCodes.Draft, PublishedAt = publish ? now : null, CreatedAt = now };
            db.Add(offering); await db.SaveChangesAsync(ct);
            foreach (var (student, qt, thi, attempt) in rows)
            {
                var e = new CourseEnrollment { OfferingId = offering.Id, ProgramCourseId = course.Id, StudentId = student.Id, AcademicPeriod = period, AttemptKind = attempt, Revision = 1, CreatedAt = now };
                db.Add(e); await db.SaveChangesAsync(ct);
                var parts = schemes[course.Id];
                db.AddRange(new StudentGrade { EnrollmentId = e.Id, ComponentId = parts[0].Id, Score = qt, UpdatedAt = now, UpdatedBy = 0 },
                    new StudentGrade { EnrollmentId = e.Id, ComponentId = parts[1].Id, Score = thi, UpdatedAt = now, UpdatedBy = 0 });
                db.AddRange(new GradeChange { EnrollmentId = e.Id, ComponentId = parts[0].Id, NewScore = qt, ActorId = 0, ChangedAt = now, Source = "import", Reason = "Dữ liệu mẫu" },
                    new GradeChange { EnrollmentId = e.Id, ComponentId = parts[1].Id, NewScore = thi, ActorId = 0, ChangedAt = now, Source = "import", Reason = "Dữ liệu mẫu" });
                if (!publish) continue;
                var outcome = Domain.GradeRules.Outcome(Domain.GradeRules.Total([(qt, 10m, 40m), (thi, 10m, 60m)], policy.CourseScoreDecimals), null, bands);
                e.FinalScore = outcome.FinalScore; e.LetterGrade = outcome.Letter; e.GradePoint = outcome.Point4; e.Passed = outcome.Passed; e.PublishedAt = now;
            }
            await db.SaveChangesAsync(ct);
        }
        if (courses.Count < 3) return;
        await Offering(courses[0], "2025-2026-HK1", true, [(students[0], 9, 8.5m, "first"), (students[1], 7, 7.5m, "first"), (students[2], 6, 6, "first"), (students[3], 2, 3, "first")]);
        await Offering(courses[1], "2025-2026-HK1", true, [(students[0], 8, 9, "first"), (students[1], 9, 9.5m, "first"), (students[2], 5, 5.5m, "first"), (students[3], 6, 5, "first")]);
        await Offering(courses[0], "2025-2026-HK2", true, [(students[3], 6, 7, "retake")]);
        await Offering(courses[2], "2025-2026-HK2", false, [(students[0], 8, 7, "first"), (students[1], 9, 8, "first"), (students[2], 7, 6, "first")]);
    }

    private static string? Text(JsonElement item, string name) => item.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value.GetString() : null;

    private static JsonDocument Read(string name)
    {
        var assembly = typeof(CurriculumDemoSeeder).Assembly;
        using var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(n => n.EndsWith(".Data." + name, StringComparison.Ordinal)))!;
        using var reader = new StreamReader(stream);
        // Git may check JSON out with CRLF on Windows; the resource checksum uses LF.
        var text = reader.ReadToEnd().ReplaceLineEndings("\n");
        if (!name.EndsWith(".manifest.json", StringComparison.Ordinal))
        {
            using var manifest = Read(name.Replace(".json", ".manifest.json"));
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
            if (hash != Text(manifest.RootElement, "resourceSha256")) throw new InvalidOperationException($"Demo resource {name} has changed; verify its provenance before updating the manifest.");
        }
        return JsonDocument.Parse(text);
    }
}
