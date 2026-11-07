using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ObeAunQa.Modules.Curriculum.Application;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

public sealed class AcademicSheetReader : IAcademicSheetReader
{
    private static readonly XNamespace S = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const int MaxFile = 5 * 1024 * 1024;
    private static void Invalid(string message) => AcademicValidator.Invalid("file", message);
    private static void FileSize(byte[] bytes) { if (bytes.Length is 0 or > MaxFile) Invalid("Tệp phải có dữ liệu và không vượt quá 5 MB."); }
    public IReadOnlyList<string> Sheets(byte[] bytes, string fileName)
    {
        FileSize(bytes);
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)) return ["Bảng dữ liệu"];
        try { using var zip = Open(bytes); return WorkbookSheets(zip).Select(s => s.Name).ToArray(); }
        catch (Exception e) when (e is InvalidDataException or XmlException or FormatException) { Invalid("Không đọc được workbook .xlsx. Hãy lưu lại tệp Excel hợp lệ."); return []; }
    }
    public IReadOnlyList<AcademicImportRow> Read(byte[] bytes, string fileName, string? sheetName, string mode, AssessmentScheme scheme)
    {
        FileSize(bytes);
        if (mode is not ("students" or "grades")) Invalid("Chọn import sinh viên hoặc điểm.");
        List<(int Number, string[] Cells)> rows;
        try
        {
            if (fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)) rows = Excel(bytes, sheetName);
            else if (new[] { ".csv", ".tsv", ".txt" }.Any(e => fileName.EndsWith(e, StringComparison.OrdinalIgnoreCase))) {
                using var reader = new StreamReader(new MemoryStream(bytes), new UTF8Encoding(false, true), true);
                rows = Delimited(reader.ReadToEnd());
            }
            else { Invalid("Hỗ trợ .xlsx, CSV UTF-8 và bảng dán từ Sheets. Tệp .xls cần lưu lại thành .xlsx."); return []; }
        }
        catch (Exception e) when (e is InvalidDataException or XmlException or FormatException or DecoderFallbackException or OverflowException) { Invalid("Tệp không hợp lệ hoặc không đọc được. Dùng .xlsx hoặc CSV UTF-8 theo mẫu."); return []; }
        rows = rows.Where(r => r.Cells.Any(c => !string.IsNullOrWhiteSpace(c))).ToList();
        if (rows.Count is < 2 or > 501) Invalid("Bảng phải có hàng tiêu đề và từ 1 đến 500 dòng dữ liệu.");
        var header = rows[0].Cells.Select(Header).ToArray();
        if (header.Length > 22 || header.Any(string.IsNullOrEmpty) || header.Distinct().Count() != header.Length) Invalid("Tiêu đề cột bị trống, trùng hoặc quá 22 cột.");
        var codeIndex = Array.IndexOf(header, "MSSV"); var nameIndex = Array.IndexOf(header, "HOTEN");
        if (codeIndex < 0 || (mode == "students" && nameIndex < 0)) Invalid("Thiếu cột MSSV hoặc HoTen. Hãy tải mẫu import.");
        var known = new HashSet<string>(scheme.Components.Select(c => c.Code), StringComparer.Ordinal) { "MSSV", "HOTEN" };
        if (mode == "students") known = ["MSSV", "HOTEN"];
        if (header.Any(h => !known.Contains(h))) Invalid("Có cột không được hỗ trợ hoặc mã đầu điểm chưa được cấu hình: " + string.Join(", ", header.Where(h => !known.Contains(h))));
        if (mode == "grades" && !header.Any(h => h is not ("MSSV" or "HOTEN"))) Invalid("Tệp điểm cần có ít nhất một cột mã đầu điểm.");
        var result = new List<AcademicImportRow>();
        foreach (var row in rows.Skip(1))
        {
            if (row.Cells.Length > header.Length && row.Cells.Skip(header.Length).Any(c => c.Trim().Length > 0)) Invalid($"Dòng {row.Number} có dữ liệu ngoài cột tiêu đề.");
            string Cell(int i) => i >= 0 && i < row.Cells.Length ? row.Cells[i].Trim() : "";
            var scores = new Dictionary<string, decimal?>();
            if (mode == "grades") for (var i = 0; i < header.Length; i++)
            {
                if (header[i] is "MSSV" or "HOTEN") continue;
                var value = Cell(i); if (value.Length == 0) continue;
                if (!decimal.TryParse(value.Replace(',', '.'), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var score)) Invalid($"Dòng {row.Number}, cột {header[i]}: điểm phải là số, ví dụ 8.5 hoặc 8,5.");
                scores.Add(header[i], score);
            }
            result.Add(new(row.Number, Cell(codeIndex), Cell(nameIndex), scores));
        }
        return result;
    }
    private static string Header(string value)
    {
        var normalized = new string(value.Trim().Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).Replace('đ', 'd').Replace('Đ', 'D').ToUpperInvariant().Replace(" ", "");
        return normalized switch { "STUDENTCODE" or "MASINHVIEN" => "MSSV", "FULLNAME" or "HOVATEN" => "HOTEN", _ => normalized };
    }
    private static ZipArchive Open(byte[] bytes)
    {
        var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        if (zip.Entries.Count > 2000 || zip.Entries.Sum(e => e.Length) > 40L * 1024 * 1024 || zip.Entries.Any(e => e.Length > 10L * 1024 * 1024)) {
            zip.Dispose(); Invalid("Workbook giải nén quá lớn. Hãy dùng tệp chỉ chứa bảng cần import.");
        }
        return zip;
    }
    private static XDocument Xml(ZipArchive zip, string path)
    {
        var entry = zip.GetEntry(path) ?? throw new InvalidDataException();
        using var stream = entry.Open(); using var xml = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 10 * 1024 * 1024 });
        return XDocument.Load(xml);
    }
    private static (string Name, string Path)[] WorkbookSheets(ZipArchive zip)
    {
        var relations = Xml(zip, "xl/_rels/workbook.xml.rels").Root!.Elements().Where(e => (string?)e.Attribute("TargetMode") != "External")
            .ToDictionary(e => (string)e.Attribute("Id")!, e => (string)e.Attribute("Target")!);
        return Xml(zip, "xl/workbook.xml").Descendants(S + "sheet").Select(s => {
            if (!relations.TryGetValue((string)s.Attribute(R + "id")!, out var target)) throw new InvalidDataException();
            var path = target.StartsWith('/') ? target.TrimStart('/') : new Uri(new Uri("https://workbook.local/xl/"), target).AbsolutePath.TrimStart('/');
            if (!path.StartsWith("xl/worksheets/", StringComparison.Ordinal) || path.Contains("..")) throw new InvalidDataException();
            return ((string)s.Attribute("name")!, path);
        }).ToArray();
    }
    private static List<(int Number, string[] Cells)> Excel(byte[] bytes, string? sheetName)
    {
        using var zip = Open(bytes); var sheets = WorkbookSheets(zip);
        var sheet = sheetName is null ? sheets.FirstOrDefault() : sheets.SingleOrDefault(s => s.Name == sheetName);
        if (sheet == default) { Invalid("Không tìm thấy sheet đã chọn."); return []; }
        var strings = zip.GetEntry("xl/sharedStrings.xml") is null ? [] : Xml(zip, "xl/sharedStrings.xml").Descendants(S + "si")
            .Select(si => string.Concat(si.Descendants(S + "t").Select(t => t.Value))).ToArray();
        var formats = new Dictionary<uint, string>(); uint[] styles = [];
        if (zip.GetEntry("xl/styles.xml") is not null) {
            var doc = Xml(zip, "xl/styles.xml");
            formats = doc.Descendants(S + "numFmt").ToDictionary(f => (uint)f.Attribute("numFmtId")!, f => (string)f.Attribute("formatCode")!);
            styles = doc.Root?.Element(S + "cellXfs")?.Elements(S + "xf").Select(f => (uint?)f.Attribute("numFmtId") ?? 0).ToArray() ?? [];
        }
        var result = new List<(int, string[])>();
        foreach (var row in Xml(zip, sheet.Path).Descendants(S + "row"))
        {
            var cells = new SortedDictionary<int, string>();
            foreach (var cell in row.Elements(S + "c"))
            {
                var reference = (string?)cell.Attribute("r") ?? throw new InvalidDataException(); int index = 0;
                foreach (var c in reference.TakeWhile(char.IsAsciiLetterUpper)) index = checked(index * 26 + c - 'A' + 1);
                if (index is < 1 or > 22) { if (!string.IsNullOrEmpty(cell.Value)) Invalid("Bảng có dữ liệu ngoài 22 cột hỗ trợ."); else continue; }
                if (cell.Element(S + "f") is not null) Invalid($"Ô {reference} chứa công thức. Hãy dán giá trị trước khi import.");
                var type = (string?)cell.Attribute("t"); var value = cell.Element(S + "v")?.Value ?? "";
                if (type == "s") { if (!int.TryParse(value, out var i) || i < 0 || i >= strings.Length) throw new InvalidDataException(); value = strings[i]; }
                else if (type == "inlineStr") value = string.Concat(cell.Descendants(S + "t").Select(t => t.Value));
                else if (type is "e" or "b") Invalid($"Ô {reference} chứa lỗi hoặc giá trị boolean.");
                else if (uint.TryParse((string?)cell.Attribute("s"), out var style) && style < styles.Length && formats.TryGetValue(styles[style], out var format)
                    && format.All(c => c == '0') && format.Length is > 1 and <= 30 && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) && number == decimal.Truncate(number))
                    value = number.ToString(new string('0', format.Length), CultureInfo.InvariantCulture);
                cells[index - 1] = value;
            }
            if (!cells.Values.Any(v => !string.IsNullOrWhiteSpace(v))) continue;
            if (result.Count > 500) Invalid("Mỗi lần chỉ import tối đa 500 dòng.");
            var values = Enumerable.Range(0, cells.Keys.Max() + 1).Select(i => cells.GetValueOrDefault(i, "")).ToArray();
            result.Add(((int?)row.Attribute("r") ?? result.Count + 1, values));
        }
        return result;
    }
    private static List<(int Number, string[] Cells)> Delimited(string text)
    {
        var first = text.Split('\n')[0]; var delimiter = first.Contains('\t') ? '\t' : first.Count(c => c == ';') > first.Count(c => c == ',') ? ';' : ',';
        var result = new List<(int, string[])>(); var row = new List<string>(); var cell = new StringBuilder(); bool quoted = false, endedQuote = false; int number = 1;
        void EndRow() { row.Add(cell.ToString()); result.Add((number++, row.ToArray())); row.Clear(); cell.Clear(); endedQuote = false; if (result.Count > 502) Invalid("Tối đa 500 dòng dữ liệu mỗi lần."); }
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted) { if (c == '"') { if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; } else { quoted = false; endedQuote = true; } } else cell.Append(c); }
            else if (c == '"' && cell.Length == 0 && !endedQuote) quoted = true;
            else if (c == delimiter) { row.Add(cell.ToString()); cell.Clear(); endedQuote = false; }
            else if (c is '\n' or '\r') { if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++; EndRow(); }
            else if (endedQuote || c == '"') Invalid("Cấu trúc dấu nháy CSV không hợp lệ.");
            else cell.Append(c);
        }
        if (quoted) Invalid("CSV thiếu dấu nháy đóng.");
        if (row.Count > 0 || cell.Length > 0 || endedQuote) EndRow();
        return result;
    }
    public byte[] Template(string mode, AssessmentScheme scheme)
    {
        var headers = new[] { "MSSV", "HoTen" }.Concat(mode == "grades" ? scheme.Components.Select(c => c.Code) : []).ToArray();
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            void Add(string name, string content) { using var w = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false)); w.Write(content); }
            Add("[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
            Add("_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            Add("xl/_rels/workbook.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
            Add("xl/workbook.xml", $"<workbook xmlns=\"{S}\" xmlns:r=\"{R}\"><sheets><sheet name=\"Import\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
            var cells = headers.Select((h, i) => new XElement(S + "c", new XAttribute("r", $"{(char)('A' + i)}1"), new XAttribute("t", "inlineStr"), new XElement(S + "is", new XElement(S + "t", h))));
            Add("xl/worksheets/sheet1.xml", new XDocument(new XElement(S + "worksheet", new XElement(S + "sheetData", new XElement(S + "row", new XAttribute("r", 1), cells)))).ToString());
        }
        return output.ToArray();
    }
}
