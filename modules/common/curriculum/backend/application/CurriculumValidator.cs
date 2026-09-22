using System.Text.RegularExpressions;

namespace ObeAunQa.Modules.Curriculum.Application;

public static partial class CurriculumValidator
{
    private const int MaximumPageSize = 100;

    public static (int Page, int PageSize, string? Search) ListQuery(
        int page,
        int pageSize,
        string? search)
    {
        var errors = new Dictionary<string, string[]>();
        if (page < 1)
        {
            errors["page"] = ["Trang phải lớn hơn hoặc bằng 1."];
        }

        if (pageSize is < 1 or > MaximumPageSize)
        {
            errors["pageSize"] = [$"Kích thước trang phải từ 1 đến {MaximumPageSize}."];
        }

        var normalizedSearch = NormalizeOptional(search);
        if (normalizedSearch?.Length > 200)
        {
            errors["q"] = ["Từ khóa tìm kiếm không được vượt quá 200 ký tự."];
        }

        ThrowIfInvalid(errors);
        return (page, pageSize, normalizedSearch);
    }

    public static (string Code, string Name) Program(CreateProgramRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        var code = NormalizeCode(request.Code, "code", errors);
        var name = NormalizeRequiredText(request.Name, "name", 2, 250, errors);
        ThrowIfInvalid(errors);
        return (code, name);
    }

    public static string ProgramName(UpdateProgramRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        var name = NormalizeRequiredText(request.Name, "name", 2, 250, errors);
        ThrowIfInvalid(errors);
        return name;
    }

    public static string VersionCode(string? value)
    {
        var errors = new Dictionary<string, string[]>();
        var code = NormalizeCode(value, "versionCode", errors);
        ThrowIfInvalid(errors);
        return code;
    }

    public static NormalizedCourse Course(
        string? institutionalCode,
        string? name,
        int credits,
        string? semester,
        int? displayOrder,
        bool requireDisplayOrder)
    {
        var errors = new Dictionary<string, string[]>();
        var normalizedCode = NormalizeOptional(institutionalCode)?.ToUpperInvariant();
        if (normalizedCode is not null &&
            (normalizedCode.Length > 50 || !CourseCodePattern().IsMatch(normalizedCode)))
        {
            errors["institutionalCode"] =
                ["Mã học phần chỉ gồm chữ, số, dấu chấm, gạch dưới hoặc gạch ngang và tối đa 50 ký tự."];
        }

        var normalizedName = NormalizeRequiredText(name, "name", 2, 250, errors);
        var normalizedSemester = NormalizeRequiredText(semester, "semester", 1, 50, errors);

        if (credits is < 1 or > 30)
        {
            errors["credits"] = ["Số tín chỉ phải từ 1 đến 30."];
        }

        if (requireDisplayOrder && displayOrder is null)
        {
            errors["displayOrder"] = ["Thứ tự hiển thị là bắt buộc."];
        }
        else if (displayOrder is <= 0)
        {
            errors["displayOrder"] = ["Thứ tự hiển thị phải lớn hơn 0."];
        }

        ThrowIfInvalid(errors);
        return new NormalizedCourse(
            normalizedCode,
            normalizedName,
            credits,
            normalizedSemester,
            displayOrder);
    }

    public static NormalizedLearningOutcome LearningOutcome(
        string? code,
        string? statement,
        string? levelCode)
    {
        var errors = new Dictionary<string, string[]>();
        var normalizedCode = NormalizeCode(code, "code", errors);
        var normalizedStatement = NormalizeRequiredText(statement, "statement", 2, 2000, errors);
        var normalizedLevelCode = NormalizeOptional(levelCode)?.ToUpperInvariant();
        if (normalizedLevelCode?.Length > 50)
        {
            errors["levelCode"] = ["Mức độ không được vượt quá 50 ký tự."];
        }

        ThrowIfInvalid(errors);
        return new NormalizedLearningOutcome(
            normalizedCode,
            normalizedStatement,
            normalizedLevelCode);
    }

    public static NormalizedCoursePloMapping CoursePloMapping(
        CreateCoursePloMappingRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.PloId <= 0)
        {
            errors["ploId"] = ["Mã định danh PLO phải lớn hơn 0."];
        }

        var values = CoursePloMappingValues(
            request.WeightCode,
            request.ProgressionCode,
            request.Fit,
            request.FitReason,
            errors);
        ThrowIfInvalid(errors);
        return new NormalizedCoursePloMapping(
            request.PloId,
            values.WeightCode,
            values.ProgressionCode,
            values.Fit,
            values.FitReason);
    }

    public static NormalizedCoursePloMappingUpdate CoursePloMapping(
        UpdateCoursePloMappingRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        var values = CoursePloMappingValues(
            request.WeightCode,
            request.ProgressionCode,
            request.Fit,
            request.FitReason,
            errors);
        ThrowIfInvalid(errors);
        return new NormalizedCoursePloMappingUpdate(
            values.WeightCode,
            values.ProgressionCode,
            values.Fit,
            values.FitReason);
    }

    private static (string WeightCode, string ProgressionCode, string? Fit, string? FitReason)
        CoursePloMappingValues(
            string? weightCode,
            string? progressionCode,
            string? fit,
            string? fitReason,
            IDictionary<string, string[]> errors)
    {
        var normalizedWeight = NormalizeOptional(weightCode)?.ToUpperInvariant() ?? string.Empty;
        if (normalizedWeight is not ("X" or "Y"))
        {
            errors["weightCode"] = ["Trọng số chỉ nhận X hoặc Y."];
        }

        var normalizedProgression = NormalizeOptional(progressionCode)?.ToUpperInvariant() ?? string.Empty;
        if (normalizedProgression is not ("I" or "R" or "E"))
        {
            errors["progressionCode"] = ["Mức tiến triển chỉ nhận I, R hoặc E."];
        }

        var normalizedFit = NormalizeOptional(fit);
        if (normalizedFit?.Length > 100)
        {
            errors["fit"] = ["Mức phù hợp không được vượt quá 100 ký tự."];
        }

        var normalizedReason = NormalizeOptional(fitReason);
        if (normalizedReason?.Length > 1000)
        {
            errors["fitReason"] = ["Lý do phù hợp không được vượt quá 1000 ký tự."];
        }

        return (normalizedWeight, normalizedProgression, normalizedFit, normalizedReason);
    }

    private static string NormalizeCode(
        string? value,
        string field,
        IDictionary<string, string[]> errors)
    {
        var normalized = NormalizeOptional(value)?.ToUpperInvariant() ?? string.Empty;
        if (normalized.Length is < 1 or > 30 || !CodePattern().IsMatch(normalized))
        {
            errors[field] =
                ["Mã phải từ 1 đến 30 ký tự và chỉ gồm chữ, số, dấu chấm, gạch dưới hoặc gạch ngang."];
        }

        return normalized;
    }

    private static string NormalizeRequiredText(
        string? value,
        string field,
        int minimumLength,
        int maximumLength,
        IDictionary<string, string[]> errors)
    {
        var normalized = NormalizeOptional(value) ?? string.Empty;
        if (normalized.Length < minimumLength || normalized.Length > maximumLength)
        {
            errors[field] =
                [$"Giá trị phải từ {minimumLength} đến {maximumLength} ký tự."];
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static void ThrowIfInvalid(IReadOnlyDictionary<string, string[]> errors)
    {
        if (errors.Count > 0)
        {
            throw new CurriculumValidationException(errors);
        }
    }

    [GeneratedRegex("^[A-Z0-9][A-Z0-9._-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex CodePattern();

    [GeneratedRegex("^[A-Z0-9][A-Z0-9._-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex CourseCodePattern();
}

public sealed record NormalizedCourse(
    string? InstitutionalCode,
    string Name,
    int Credits,
    string Semester,
    int? DisplayOrder);

public sealed record NormalizedLearningOutcome(
    string Code,
    string Statement,
    string? LevelCode);

public sealed record NormalizedCoursePloMapping(
    long PloId,
    string WeightCode,
    string ProgressionCode,
    string? Fit,
    string? FitReason);

public sealed record NormalizedCoursePloMappingUpdate(
    string WeightCode,
    string ProgressionCode,
    string? Fit,
    string? FitReason);
