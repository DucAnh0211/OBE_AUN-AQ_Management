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
        var program = new CurriculumProgram { Code = "CS", Name = "Khoa học máy tính" };
        db.Programs.Add(program);
        db.SourceDocuments.AddRange(workbook, syllabus);
        await db.SaveChangesAsync(ct);

        var baseline = await AddVersionAsync(program.Id, first, m1.RootElement, workbook, syllabus, null, ct);
        await AddVersionAsync(program.Id, v2.RootElement, m2.RootElement, workbook, syllabus, baseline, ct);
        await new CurriculumDemoVerifier(db).VerifyAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    private async Task<ProgramVersion> AddVersionAsync(long programId, JsonElement data, JsonElement manifest,
        SourceDocument workbook, SourceDocument syllabus, ProgramVersion? source, CancellationToken ct)
    {
        var isBaseline = source == null;
        var version = new ProgramVersion { ProgramId = programId, VersionCode = Text(data.GetProperty("metadata"), "program_version")!,
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
                Course = original?.Course ?? new Course { Name = Text(item, "name")! },
                Credits = original?.Credits ?? item.GetProperty("credits").GetInt32(),
                CourseCodeSourceValue = original?.CourseCodeSourceValue ?? Text(item, "course_code_source_value"),
                SemesterSourceValue = original?.SemesterSourceValue ?? Text(item, "semester_source_value"),
                DatasetStatus = isBaseline ? Text(item, "dataset_status")! : "draft",
                QualityFlags = original?.QualityFlags ?? flags.GetRawText(),
                CourseNameSnapshot = original?.CourseNameSnapshot ?? Text(item, "name")!,
                CourseCodeSnapshot = original?.CourseCodeSnapshot ?? (flags.EnumerateArray().Any(f => f.GetString() == "course_code_missing_or_placeholder") || string.IsNullOrWhiteSpace(code) ? null : code),
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
