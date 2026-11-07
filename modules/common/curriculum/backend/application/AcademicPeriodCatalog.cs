namespace ObeAunQa.Modules.Curriculum.Application;

public static class AcademicPeriodCatalog
{
    public static IReadOnlyList<AcademicPeriodBatch> Build(IEnumerable<string> existing, int currentYear)
    {
        var batches = new Dictionary<string, Dictionary<string, AcademicSemesterOption>>(StringComparer.Ordinal);
        void Add(string batch, string name, string period)
        {
            if (!batches.TryGetValue(batch, out var semesters)) batches[batch] = semesters = new(StringComparer.Ordinal);
            semesters[period] = new(name, period);
        }
        // Nearby academic years allow opening a new roster before its first enrollment exists.
        for (var year = currentYear - 4; year <= currentYear + 2; year++)
        {
            var batch = $"{year}-{year + 1}";
            foreach (var semester in new[] { "1", "2" }) Add(batch, $"Học kỳ {semester}", $"{batch}-HK{semester}");
            Add(batch, "Học kỳ hè", $"{batch}-HKHE");
        }
        foreach (var period in existing.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.Ordinal))
        {
            var separator = period.LastIndexOf("-HK", StringComparison.OrdinalIgnoreCase);
            if (separator > 0 && separator + 3 < period.Length)
            {
                var batch = period[..separator]; var semester = period[(separator + 3)..];
                var label = semester.ToUpperInvariant() is "HE" or "HÈ" ? "Học kỳ hè" : $"Học kỳ {semester}";
                Add(batch, label, period);
            }
            else Add(period, "Dữ liệu đã nhập", period);
        }
        return batches.OrderByDescending(p => p.Key, StringComparer.Ordinal).Select(p =>
            new AcademicPeriodBatch(p.Key, p.Value.Values.OrderBy(s => s.Name, StringComparer.Ordinal).ThenBy(s => s.Period, StringComparer.Ordinal).ToArray())).ToArray();
    }
}
