namespace ObeAunQa.Modules.Curriculum.Application;

public sealed class CurriculumValidationException(
    IReadOnlyDictionary<string, string[]> errors)
    : Exception("Dữ liệu gửi lên không hợp lệ.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
