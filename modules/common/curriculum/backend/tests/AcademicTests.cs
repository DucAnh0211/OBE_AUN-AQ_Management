using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Infrastructure;
namespace ObeAunQa.Modules.Curriculum.Tests;

public sealed class AcademicTests
{
    [Fact]
    public void PeriodCatalogGroupsExistingSemestersWithoutChangingStoredKeys()
    {
        var source = new[] { "2026-2027-HK1", "2026-2027-HK2", "2026-2027-HKHE", "2010-2011-HK2", "2026-HK1", "Dot-cu-rieng" };
        var batches = AcademicPeriodCatalog.Build(source, 2026);
        var year = Assert.Single(batches, b => b.Name == "2026-2027");
        Assert.Equal(3, year.Semesters.Count);
        Assert.Contains(year.Semesters, s => s.Name == "Học kỳ hè" && s.Period == "2026-2027-HKHE");
        foreach (var key in source) Assert.Single(batches.SelectMany(b => b.Semesters), s => s.Period == key);
        Assert.Contains(batches, b => b.Name == "2010-2011");
        Assert.Equal("Dữ liệu đã nhập", batches.Single(b => b.Name == "Dot-cu-rieng").Semesters[0].Name);
    }
    [Fact]
    public void PeriodCatalogOffersNewYearsAndDoesNotMixSemestersAcrossBatches()
    {
        var batches = AcademicPeriodCatalog.Build(["2026-special-HK4"], 2026);
        Assert.Equal(new[] { "2026-2027-HK1", "2026-2027-HK2", "2026-2027-HKHE" }, batches.Single(b => b.Name == "2026-2027").Semesters.Select(s => s.Period));
        Assert.Single(batches.Single(b => b.Name == "2026-special").Semesters);
        Assert.DoesNotContain(batches.Single(b => b.Name == "2025-2026").Semesters, s => s.Period.StartsWith("2026-2027"));
    }
    [Fact]
    public async Task ValidationHttpResponseIncludesTheSpecificFieldErrors()
    {
        var type = typeof(CurriculumService).Assembly.GetType("ObeAunQa.Modules.Curriculum.Presentation.CurriculumExceptionHandler")!;
        var handler = (Microsoft.AspNetCore.Diagnostics.IExceptionHandler)Activator.CreateInstance(type, nonPublic: true)!;
        var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        context.RequestServices = new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider();
        context.Response.Body = new MemoryStream();
        var exception = new CurriculumValidationException(new Dictionary<string, string[]> { ["components"] = ["Tổng trọng số các đầu điểm phải bằng 100%."] });
        Assert.True(await handler.TryHandleAsync(context, exception, default));
        Assert.Equal(400, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var response = await System.Text.Json.JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("Tổng trọng số các đầu điểm phải bằng 100%.", response.RootElement.GetProperty("errors").GetProperty("components")[0].GetString());
    }
    private readonly AcademicSheetReader reader = new();
    private static AssessmentScheme Scheme => new(1, 1, false, [new("QT", "Quá trình", 40, 10), new("THI", "Thi kết thúc", 60, 100)]);
    [Fact] public void TotalNormalizesScalesAndDistinguishesZeroFromMissing()
    {
        Assert.Equal(8m, GradeRules.Total([(8m, 10m, 40m), (80m, 100m, 60m)]));
        Assert.Equal(0m, GradeRules.Total([(0m, 10m, 100m)]));
        Assert.Null(GradeRules.Total([(null, 10m, 100m)])); Assert.Null(GradeRules.Total([]));
    }
    [Fact] public void SchemeRequiresExactHundredPercentAndUniqueCodes()
    {
        AcademicValidator.Scheme(new(0, Scheme.Components));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scheme(new(0, [new("QT", "Quá trình", 20, 10)])));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scheme(new(0, [new("QT", "Quá trình", 50, 10), new("QT", "Khác", 50, 10)])));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scheme(new(0, [new("MSSV", "Tên", 100, 10)])));
    }
    [Fact] public void GradesRejectOutOfRangeUnknownAndExcessPrecision()
    {
        AcademicValidator.Scores(new Dictionary<string, decimal?> { ["QT"] = 0, ["THI"] = null }, Scheme);
        foreach (var value in new[] { -1m, 10.01m, 8.123m })
            Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scores(new Dictionary<string, decimal?> { ["QT"] = value }, Scheme));
        Assert.Throws<CurriculumValidationException>(() => AcademicValidator.Scores(new Dictionary<string, decimal?> { ["UNKNOWN"] = 3 }, Scheme));
    }
    [Fact] public void CsvAndSheetsPreserveStudentCodeUnicodeAndDecimalComma()
    {
        var rows = reader.Read(Encoding.UTF8.GetBytes("Mã sinh viên\tHọ và tên\tQT\tTHI\n001234\tNguyễn Văn An\t8,5\t0\n002345\tLê Hà\t\t75"), "pasted.tsv", null, "grades", Scheme);
        Assert.Equal("001234", rows[0].StudentCode); Assert.Equal("Nguyễn Văn An", rows[0].FullName);
        Assert.Equal(8.5m, rows[0].Scores["QT"]); Assert.Equal(0m, rows[0].Scores["THI"]); Assert.DoesNotContain("QT", rows[1].Scores.Keys);
        var csv = reader.Read(Encoding.UTF8.GetBytes("MSSV,HoTen\r\n001,\"Nguyễn, Văn An\""), "students.csv", null, "students", Scheme);
        Assert.Equal("Nguyễn, Văn An", csv[0].FullName);
    }
    [Theory]
    [InlineData("MSSV\tHoTen\tBAD\nS01\tHọ tên\t8")]
    [InlineData("MSSV\tMSSV\nS01\tS02")]
    [InlineData("MSSV\tQT\nS01\t=1+2")]
    public void InvalidSheetsAreRejected(string text) => Assert.Throws<CurriculumValidationException>(() => reader.Read(Encoding.UTF8.GetBytes(text), "sheet.tsv", null, "grades", Scheme));
    [Fact] public void XlsxUsesChosenSheetAndRejectsFormulas()
    {
        var bytes = Workbook(false);
        Assert.Equal(new[] { "Sinh vien", "Diem" }, reader.Sheets(bytes, "test.xlsx"));
        var rows = reader.Read(bytes, "test.xlsx", "Diem", "grades", Scheme);
        Assert.Equal("001234", rows[0].StudentCode); Assert.Equal(9m, rows[0].Scores["QT"]);
        Assert.Throws<CurriculumValidationException>(() => reader.Read(Workbook(true), "test.xlsx", "Diem", "grades", Scheme));
    }
    [Fact] public void TemplateIsValidExcelAndContainsConfiguredHeaders()
    {
        var bytes = reader.Template("grades", Scheme);
        Assert.Equal("Import", Assert.Single(reader.Sheets(bytes, "template.xlsx")));
        using var zip = new ZipArchive(new MemoryStream(bytes)); using var r = new StreamReader(zip.GetEntry("xl/worksheets/sheet1.xml")!.Open());
        var text = r.ReadToEnd(); Assert.Contains("QT", text); Assert.Contains("THI", text);
    }
    private static byte[] Workbook(bool formula)
    {
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true)) {
            void Add(string name, string xml) { using var w = new StreamWriter(zip.CreateEntry(name).Open()); w.Write(xml); }
            Add("xl/workbook.xml", "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Sinh vien\" r:id=\"r1\"/><sheet name=\"Diem\" r:id=\"r2\"/></sheets></workbook>");
            Add("xl/_rels/workbook.xml.rels", "<Relationships><Relationship Id=\"r1\" Target=\"worksheets/s1.xml\"/><Relationship Id=\"r2\" Target=\"worksheets/s2.xml\"/></Relationships>");
            Add("xl/styles.xml", "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><numFmts><numFmt numFmtId=\"164\" formatCode=\"000000\"/></numFmts><cellXfs><xf numFmtId=\"0\"/><xf numFmtId=\"164\"/></cellXfs></styleSheet>");
            var cells = "<row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>MSSV</t></is></c><c r=\"B1\" t=\"inlineStr\"><is><t>QT</t></is></c></row>";
            var row = "<row r=\"2\"><c r=\"A2\" s=\"1\"><v>1234</v></c><c r=\"B2\">" + (formula ? "<f>1+8</f>" : "") + "<v>9</v></c></row>";
            Add("xl/worksheets/s1.xml", "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>" + cells + "</sheetData></worksheet>");
            Add("xl/worksheets/s2.xml", "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>" + cells + row + "</sheetData></worksheet>");
        }
        return output.ToArray();
    }
}
