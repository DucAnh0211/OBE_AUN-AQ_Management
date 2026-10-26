using System.Reflection;
using PdfSharp.Fonts;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

internal sealed class ReportFontResolver : IFontResolver
{
    public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic) =>
        new(bold ? "NotoSans-Bold.ttf" : "NotoSans-Regular.ttf", false, italic);
    public byte[] GetFont(string faceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(n => n.EndsWith(faceName, StringComparison.Ordinal)))!;
        using var output = new MemoryStream();
        stream.CopyTo(output);
        return output.ToArray();
    }
}
