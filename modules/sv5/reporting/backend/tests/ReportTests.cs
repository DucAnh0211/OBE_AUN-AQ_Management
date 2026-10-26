using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Reporting.Application;
using ObeAunQa.Modules.Reporting.Infrastructure;
using PdfSharp.Pdf.IO;

namespace ObeAunQa.Modules.Reporting.Tests;

public sealed class ReportTests
{
    private static readonly DateTime Date = new(2026, 10, 26, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTimeOffset Now = new(Date);

    [Fact]
    public void CountsDistinctClosAndSeparatesCourseMappingFromCloSupport()
    {
        var source = Fixture();
        source = source with { CloMappings = [..source.CloMappings, source.CloMappings[0]] };
        var report = ReportBuilder.Build(new("plo", 10), source, "tester", Now);
        Assert.Equal(new ReportMetric("CLO có mapping", 2, 3, 66.67m), report.Metrics[0]);
        Assert.Equal(1, report.Metrics[1].Numerator);
        Assert.Equal(1, report.Metrics[2].Numerator);
        Assert.Equal(1, report.Metrics[3].Numerator);
        var totals = report.Tables.Single(t => t.Title == "Tổng hợp PLO");
        Assert.Equal(new[] { "PLO1", "1", "2", "2", "6" }, totals.Rows[0]);
        Assert.Equal(new[] { "PLO2", "1", "0", "0", "2" }, totals.Rows[1]);
        Assert.Contains(report.Findings, f => f.Contains("có liên kết học phần–PLO nhưng chưa có CLO hỗ trợ"));
    }

    [Fact]
    public void SameCloCodeInDifferentCoursesDoesNotMergeAndCourseFilterWorks()
    {
        var report = ReportBuilder.Build(new("clo", 10, 102), Fixture(), "tester", Now);
        Assert.Single(report.Source.Clos);
        Assert.Equal(0, report.Metrics[0].Numerator);
        Assert.Equal(1, report.Metrics[0].Denominator);
        Assert.Contains(report.Findings, f => f.Contains("HP102") && f.Contains("CLO1"));
    }

    [Fact]
    public void EmptyDataDoesNotBecomeOneHundredPercent()
    {
        var empty = Fixture() with { Courses = [], Plos = [], Clos = [], CourseMappings = [], CloMappings = [] };
        var report = ReportBuilder.Build(new("plo", 10), empty, "tester", Now);
        Assert.All(report.Metrics, m => { Assert.Null(m.Percentage); Assert.Equal(0, m.Denominator); });
    }

    [Fact]
    public void ArchivedAndCrossVersionRecordsDoNotAffectResults()
    {
        var source = Fixture();
        source = source with { Courses = [..source.Courses, Course(103) with { ArchivedAt = Date }, Course(104) with { ProgramVersionId = 20 }],
            Clos = [..source.Clos, new(4, 103, "CLO1", "Archived", null, "test", "finalized"), new(5, 104, "CLO1", "Other version", null, "test", "finalized")],
            CourseMappings = [..source.CourseMappings, new(203, 20, 104, 1, "PLO1", "X", "E", "test", null, null)] };
        var report = ReportBuilder.Build(new("plo", 10), source, "tester", Now);
        Assert.Equal(2, report.Source.Courses.Count);
        Assert.Equal(3, report.Source.Clos.Count);
        Assert.Equal(2, report.Source.CourseMappings.Count);
    }

    [Theory]
    [InlineData("unknown", 10, null)]
    [InlineData("course", 10, null)]
    [InlineData("plo", 10, 101L)]
    [InlineData("clo", -1, null)]
    public void InvalidScopeIsRejected(string kind, long version, long? course) =>
        Assert.Equal(400, Assert.Throws<ReportException>(() => ReportBuilder.Validate(new(kind, version, course))).StatusCode);

    [Fact]
    public void CourseFromAnotherVersionIsNotFound() =>
        Assert.Equal(404, Assert.Throws<ReportException>(() => ReportBuilder.Build(new("course", 10, 999), Fixture(), "tester", Now)).StatusCode);

    [Fact]
    public void SnapshotSurvivesSerializationAndLaterSourceChanges()
    {
        var source = Fixture();
        var old = ReportBuilder.Build(new("clo", 10), source, "tester", Now);
        var saved = JsonSerializer.Deserialize<ReportDocument>(JsonSerializer.Serialize(old, ReportBuilder.JsonOptions), ReportBuilder.JsonOptions)!;
        source = source with { Clos = source.Clos.Select(c => c with { Statement = "Changed" }).ToArray() };
        var newer = ReportBuilder.Build(new("clo", 10), source, "tester", Now.AddMinutes(1));
        Assert.NotEqual(saved.SnapshotHash, newer.SnapshotHash);
        Assert.DoesNotContain(saved.Source.Clos, c => c.Statement == "Changed");
        Assert.Equal(old.SnapshotHash, saved.SnapshotHash);
    }

    [Fact]
    public void ExportsContainVietnameseAndXmlEscapesUserContent()
    {
        var source = Fixture();
        source = source with { Clos = source.Clos.Select(c => c with { Statement = "Phân tích <dữ liệu> & thiết kế hệ thống " + new string('a', 1200) }).ToArray() };
        var report = ReportBuilder.Build(new("clo", 10), source, "tester", Now);
        var exporter = new ReportExporter();
        using var docx = new MemoryStream(exporter.Docx(report));
        using var zip = new ZipArchive(docx);
        using var xmlStream = zip.GetEntry("word/document.xml")!.Open();
        var xml = XDocument.Load(xmlStream);
        Assert.Contains("Phân tích <dữ liệu> & thiết kế", xml.Root!.Value);
        Assert.Contains(report.SnapshotHash, xml.Root.Value);
        Assert.NotNull(zip.GetEntry("word/styles.xml"));
        using var pdf = new MemoryStream(exporter.Pdf(report));
        using var parsed = PdfReader.Open(pdf, PdfDocumentOpenMode.Import);
        Assert.True(parsed.PageCount >= 1);
        Assert.Equal(report.Title, parsed.Info.Title);
        var output = Environment.GetEnvironmentVariable("REPORT_TEST_ARTIFACTS");
        if (!string.IsNullOrEmpty(output))
        {
            Directory.CreateDirectory(output);
            File.WriteAllBytes(Path.Combine(output, "sample.docx"), docx.ToArray());
            File.WriteAllBytes(Path.Combine(output, "sample.pdf"), pdf.ToArray());
        }
    }

    [Fact]
    public void CurrentAccessConfigurationControlsTokenAndScopes()
    {
        const string token = "test-only-key-with-at-least-thirty-two-characters";
        var options = new ReportingAccessOptions { Users = [new() { Id = "reader", ProgramIds = [1], CanGenerate = false,
            TokenSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))) }] };
        Assert.NotNull(ReportingAuthenticationHandler.FindUser(options, token));
        Assert.Null(ReportingAuthenticationHandler.FindUser(options, token + "wrong"));
        options.Users.Clear();
        Assert.Null(ReportingAuthenticationHandler.FindUser(options, token));
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("reporting:program", "1")], "test"));
        ReportPermissions.Demand(user, 1);
        Assert.Equal(403, Assert.Throws<ReportException>(() => ReportPermissions.Demand(user, 2)).StatusCode);
        Assert.Equal(403, Assert.Throws<ReportException>(() => ReportPermissions.Demand(user, 1, true)).StatusCode);
        Assert.Equal(401, Assert.Throws<ReportException>(() => ReportPermissions.Demand(new ClaimsPrincipal(), 1)).StatusCode);
    }

    private static ProgramCourseResponse Course(long id) => new(id, 10, id, "HP" + id, "Học phần " + id,
        id == 101 ? 3 : 2, "1", (int)id, "draft", null, null, Date, Date);

    private static CurriculumReportSnapshot Fixture() => new(
        new(1, "CNTT", "Công nghệ thông tin", false, 1, 10, "V1", Date, Date),
        new(10, 1, "V1", "draft", false, null, 2, null, null, Date, Date),
        [Course(101), Course(102)],
        [new(1, 10, "PLO1", "Phân tích dữ liệu", "C4", "official"), new(2, 10, "PLO2", "Giao tiếp", null, "official")],
        [new(1, 101, "CLO1", "Phân tích", "C4", "original", "draft"), new(2, 101, "CLO2", "Thiết kế", "C6", "generated", "draft"),
            new(3, 102, "CLO1", "Trình bày", null, "original", "draft")],
        [new(201, 10, 101, 1, "PLO1", "X", "I", "original", null, null), new(202, 10, 102, 2, "PLO2", "Y", "E", "original", null, null)],
        [new(1, "CLO1", 201, 1, "PLO1"), new(2, "CLO2", 201, 1, "PLO1")]);
}
