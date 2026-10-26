using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using ObeAunQa.Modules.Reporting.Application;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

public sealed class ReportExporter : IReportExporter
{
    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private static readonly object PdfLock = new();

    public byte[] Docx(ReportDocument report)
    {
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            Add(zip, "[Content_Types].xml", new XDocument(new XElement(XName.Get("Types", "http://schemas.openxmlformats.org/package/2006/content-types"),
                new XElement(XName.Get("Default", "http://schemas.openxmlformats.org/package/2006/content-types"), new XAttribute("Extension", "rels"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                new XElement(XName.Get("Default", "http://schemas.openxmlformats.org/package/2006/content-types"), new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")),
                Override("/word/document.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"),
                Override("/word/styles.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"),
                Override("/word/footer.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"))));
            XNamespace rel = "http://schemas.openxmlformats.org/package/2006/relationships";
            Add(zip, "_rels/.rels", new XDocument(new XElement(rel + "Relationships", Relationship("rId1", "officeDocument", "word/document.xml"))));
            Add(zip, "word/_rels/document.xml.rels", new XDocument(new XElement(rel + "Relationships",
                Relationship("styles", "styles", "styles.xml"), Relationship("footer", "footer", "footer.xml"))));
            Add(zip, "word/styles.xml", new XDocument(new XElement(W + "styles",
                new XElement(W + "docDefaults", new XElement(W + "rPrDefault", new XElement(W + "rPr",
                    new XElement(W + "rFonts", new XAttribute(W + "ascii", "Arial"), new XAttribute(W + "hAnsi", "Arial")),
                    new XElement(W + "sz", new XAttribute(W + "val", "20")), new XElement(W + "lang", new XAttribute(W + "val", "vi-VN"))))))));
            var body = new XElement(W + "body");
            body.Add(Paragraph(report.Title, true, 36));
            foreach (var text in Metadata(report)) body.Add(Paragraph(text));
            body.Add(Paragraph("Chỉ số cấu trúc", true, 26));
            foreach (var m in report.Metrics) body.Add(Paragraph(MetricText(m)));
            body.Add(Paragraph("Kết quả kiểm tra", true, 26));
            foreach (var text in report.Findings) body.Add(Paragraph("• " + text));
            foreach (var table in report.Tables)
            {
                body.Add(Paragraph(table.Title, true, 26));
                if (table.Rows.Length == 0) { body.Add(Paragraph("Chưa có dữ liệu.")); continue; }
                var width = 14600 / table.Headers.Length;
                var xmlTable = new XElement(W + "tbl",
                    new XElement(W + "tblPr", new XElement(W + "tblW", new XAttribute(W + "w", "14600"), new XAttribute(W + "type", "dxa")),
                        new XElement(W + "tblLayout", new XAttribute(W + "type", "fixed")),
                        new XElement(W + "tblBorders", new[] { "top", "left", "bottom", "right", "insideH", "insideV" }.Select(b =>
                            new XElement(W + b, new XAttribute(W + "val", "single"), new XAttribute(W + "sz", "4"), new XAttribute(W + "color", "CBD5D1")))),
                        new XElement(W + "tblCellMar", new[] { "top", "left", "bottom", "right" }.Select(b =>
                            new XElement(W + b, new XAttribute(W + "w", "90"), new XAttribute(W + "type", "dxa"))))),
                    new XElement(W + "tblGrid", table.Headers.Select(_ => new XElement(W + "gridCol", new XAttribute(W + "w", width)))));
                xmlTable.Add(TableRow(table.Headers, width, true));
                foreach (var row in table.Rows.SelectMany(SplitRow)) xmlTable.Add(TableRow(row, width, false));
                body.Add(xmlTable);
                body.Add(Paragraph(""));
            }
            body.Add(Paragraph("Nguồn và giới hạn", true, 26));
            foreach (var note in report.Notes) body.Add(Paragraph(note));
            XNamespace r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            body.Add(new XElement(W + "sectPr",
                new XElement(W + "footerReference", new XAttribute(W + "type", "default"), new XAttribute(r + "id", "footer")),
                new XElement(W + "pgSz", new XAttribute(W + "w", "16838"), new XAttribute(W + "h", "11906"), new XAttribute(W + "orient", "landscape")),
                new XElement(W + "pgMar", new XAttribute(W + "top", "1000"), new XAttribute(W + "bottom", "1000"),
                    new XAttribute(W + "left", "1100"), new XAttribute(W + "right", "1100"), new XAttribute(W + "footer", "500"))));
            Add(zip, "word/document.xml", new XDocument(new XElement(W + "document", new XAttribute(XNamespace.Xmlns + "w", W), body)));
            Add(zip, "word/footer.xml", new XDocument(new XElement(W + "ftr", new XElement(W + "p",
                new XElement(W + "r", new XElement(W + "t", "Bản chưa phê duyệt • Trang ")),
                new XElement(W + "fldSimple", new XAttribute(W + "instr", "PAGE"), new XElement(W + "r", new XElement(W + "t", "1")))))));
        }
        return output.ToArray();
    }

    public byte[] Pdf(ReportDocument report)
    {
        lock (PdfLock)
        {
            GlobalFontSettings.FontResolver ??= new ReportFontResolver();
            var document = new Document();
            document.Info.Title = report.Title;
            document.Info.Author = report.CreatedBy;
            var normal = document.Styles[StyleNames.Normal]!;
            normal.Font.Name = "Noto Sans";
            normal.Font.Size = 9;
            normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(5);
            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.TopMargin = section.PageSetup.BottomMargin = Unit.FromCentimeter(1.7);
            section.PageSetup.LeftMargin = section.PageSetup.RightMargin = Unit.FromCentimeter(1.9);
            var footer = section.Footers.Primary.AddParagraph("Bản chưa phê duyệt • Trang ");
            footer.AddPageField();
            footer.Format.Alignment = ParagraphAlignment.Right;
            Heading(section, report.Title, 18);
            foreach (var text in Metadata(report)) section.AddParagraph(text);
            Heading(section, "Chỉ số cấu trúc", 12);
            foreach (var m in report.Metrics) section.AddParagraph(MetricText(m));
            Heading(section, "Kết quả kiểm tra", 12);
            foreach (var text in report.Findings) section.AddParagraph("• " + text);
            foreach (var source in report.Tables)
            {
                Heading(section, source.Title, 12);
                if (source.Rows.Length == 0) { section.AddParagraph("Chưa có dữ liệu."); continue; }
                var table = section.AddTable();
                table.Borders.Width = 0.3;
                table.Borders.Color = Colors.LightGray;
                table.TopPadding = table.BottomPadding = Unit.FromPoint(3);
                foreach (var _ in source.Headers) table.AddColumn(Unit.FromCentimeter(25.9 / source.Headers.Length));
                var header = table.AddRow();
                header.HeadingFormat = true;
                header.Format.Font.Bold = true;
                header.Shading.Color = Color.FromRgb(234, 242, 237);
                for (var i = 0; i < source.Headers.Length; i++) header.Cells[i].AddParagraph(source.Headers[i]);
                foreach (var cells in source.Rows.SelectMany(SplitRow))
                {
                    var row = table.AddRow();
                    for (var i = 0; i < cells.Length; i++) row.Cells[i].AddParagraph(cells[i]);
                }
                section.AddParagraph();
            }
            Heading(section, "Nguồn và giới hạn", 12);
            foreach (var note in report.Notes) section.AddParagraph(note);
            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            using var output = new MemoryStream();
            renderer.PdfDocument.Save(output, false);
            return output.ToArray();
        }
    }

    private static string[] Metadata(ReportDocument r) => [
        $"Chương trình: {r.ProgramCode} — {r.ProgramName}",
        $"Phiên bản: {r.VersionCode} (ID {r.VersionId}) • Trạng thái nguồn: {r.VersionStatus} • Bản báo cáo chưa phê duyệt",
        $"Mã báo cáo: {r.Id} • Người tạo: {r.CreatedBy}",
        $"Ngày tạo (UTC+07:00): {r.CreatedAt.ToOffset(TimeSpan.FromHours(7)):dd/MM/yyyy HH:mm:ss} • Quy tắc: {r.RuleVersion}",
        $"SHA-256 snapshot: {r.SnapshotHash}"
    ];
    private static string MetricText(ReportMetric m) => $"{m.Label}: {m.Numerator}/{m.Denominator} — "
        + (m.Percentage?.ToString("0.##", CultureInfo.GetCultureInfo("vi-VN")) is { } p ? $"{p}%" : "Chưa có dữ liệu");
    private static void Heading(Section section, string text, int size)
    {
        var p = section.AddParagraph(text);
        p.Format.Font.Size = size;
        p.Format.Font.Bold = true;
        p.Format.KeepWithNext = true;
        p.Format.SpaceBefore = Unit.FromPoint(10);
        p.Format.SpaceAfter = Unit.FromPoint(6);
    }

    // Split long cells into continuation rows, so neither Word nor PDF clips a row taller than a page.
    private static IEnumerable<string[]> SplitRow(string[] row)
    {
        var pieces = row.Select(text => SplitText(text.Replace("✓", "Có"))).ToArray();
        for (var part = 0; part < pieces.Max(p => p.Length); part++)
            yield return pieces.Select(p => part < p.Length ? p[part] : "").ToArray();
    }
    private static string[] SplitText(string text)
    {
        var parts = new List<string>();
        while (text.Length > 300)
        {
            var end = text.LastIndexOf(' ', 300, 120);
            if (end < 0) end = 300;
            parts.Add(text[..end]);
            text = text[end..].TrimStart();
        }
        parts.Add(text);
        return parts.ToArray();
    }
    private static XElement Paragraph(string text, bool bold = false, int size = 20) => new(W + "p",
        new XElement(W + "pPr", new XElement(W + "spacing", new XAttribute(W + "after", "100")), bold ? new XElement(W + "keepNext") : null),
        new XElement(W + "r", new XElement(W + "rPr", bold ? new XElement(W + "b") : null, new XElement(W + "sz", new XAttribute(W + "val", size))),
            new XElement(W + "t", new XAttribute(XNamespace.Xml + "space", "preserve"), text)));
    private static XElement TableRow(string[] cells, int width, bool header) => new(W + "tr",
        new XElement(W + "trPr", new XElement(W + "cantSplit"), header ? new XElement(W + "tblHeader") : null),
        cells.Select(c => new XElement(W + "tc", new XElement(W + "tcPr",
            new XElement(W + "tcW", new XAttribute(W + "w", width), new XAttribute(W + "type", "dxa")),
            header ? new XElement(W + "shd", new XAttribute(W + "fill", "EAF2ED")) : null), Paragraph(c, header, 18))));
    private static XElement Override(string name, string type) => new(XName.Get("Override", "http://schemas.openxmlformats.org/package/2006/content-types"),
        new XAttribute("PartName", name), new XAttribute("ContentType", type));
    private static XElement Relationship(string id, string type, string target) => new(XName.Get("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships"),
        new XAttribute("Id", id), new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/" + type), new XAttribute("Target", target));
    private static void Add(ZipArchive zip, string path, XDocument document)
    {
        using var stream = zip.CreateEntry(path).Open();
        document.Save(stream);
    }
}

