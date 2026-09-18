namespace ObeAunQa.Modules.Curriculum.Application;

public sealed class CurriculumValidationException(
    IReadOnlyDictionary<string, string[]> errors)
    : Exception("Dữ liệu gửi lên không hợp lệ.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class CurriculumNotFoundException(string resource, long id)
    : Exception($"Không tìm thấy {resource} có mã định danh {id}.")
{
    public string Resource { get; } = resource;

    public long ResourceId { get; } = id;
}

public sealed class CurriculumConflictException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}
