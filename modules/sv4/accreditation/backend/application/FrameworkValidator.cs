namespace ObeAunQa.Modules.Accreditation.Application;

public static class FrameworkValidator
{
    private static readonly int[] V4Counts = [5, 7, 6, 7, 8, 6, 9, 5];

    public static ImportValidationResult Validate(FrameworkImportRequest request)
    {
        var errors = new Dictionary<string, List<string>>();
        void Add(string key, string value)
        {
            if (!errors.TryGetValue(key, out var values)) errors[key] = values = [];
            values.Add(value);
        }

        if (string.IsNullOrWhiteSpace(request.Code)) Add("code", "Mã framework là bắt buộc.");
        if (string.IsNullOrWhiteSpace(request.Version)) Add("version", "Phiên bản là bắt buộc.");
        if (string.IsNullOrWhiteSpace(request.SourceTitle)) Add("sourceTitle", "Tên nguồn là bắt buộc.");
        var criteria = request.Criteria ?? [];
        foreach (var group in criteria.GroupBy(x => x.Code?.Trim(), StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
            Add("criteria", $"Mã criterion '{group.Key}' bị trùng.");

        for (var i = 0; i < criteria.Count; i++)
        {
            var criterion = criteria[i];
            var path = $"criteria[{i}]";
            if (string.IsNullOrWhiteSpace(criterion.Code)) Add($"{path}.code", "Mã criterion là bắt buộc.");
            if (criterion.DisplayOrder < 1) Add($"{path}.displayOrder", "Thứ tự phải lớn hơn 0.");
            var requirements = criterion.Requirements ?? [];
            if (requirements.Count == 0) Add($"{path}.requirements", "Criterion phải có requirement.");
            foreach (var group in requirements.GroupBy(x => x.Code?.Trim(), StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
                Add($"{path}.requirements", $"Mã requirement '{group.Key}' bị trùng.");
            for (var j = 0; j < requirements.Count; j++)
            {
                var requirement = requirements[j];
                if (string.IsNullOrWhiteSpace(requirement.Code)) Add($"{path}.requirements[{j}].code", "Mã requirement là bắt buộc.");
                else if (!requirement.Code.StartsWith($"{criterion.Code}.", StringComparison.Ordinal))
                    Add($"{path}.requirements[{j}].code", "Mã requirement không thuộc criterion.");
                if (requirement.DisplayOrder < 1) Add($"{path}.requirements[{j}].displayOrder", "Thứ tự phải lớn hơn 0.");
            }
        }

        if (request.Code?.Trim().Equals("AUN-QA-PROGRAMME", StringComparison.OrdinalIgnoreCase) == true && request.Version?.Trim() == "4.0")
        {
            if (criteria.Count != 8) Add("criteria", $"AUN-QA v4.0 phải có 8 criteria, hiện có {criteria.Count}.");
            for (var i = 0; i < Math.Min(criteria.Count, 8); i++)
            {
                var actual = criteria.FirstOrDefault(x => x.Code?.Trim() == (i + 1).ToString())?.Requirements?.Count ?? 0;
                if (actual != V4Counts[i]) Add($"criteria[{i + 1}]", $"Criterion {i + 1} phải có {V4Counts[i]} requirements, hiện có {actual}.");
            }
        }

        return new ImportValidationResult(errors.Count == 0, request.Code?.Trim(), request.Version?.Trim(),
            criteria.Count, criteria.Sum(x => x.Requirements?.Count ?? 0),
            errors.ToDictionary(x => x.Key, x => x.Value.ToArray()));
    }
}
