namespace ObeAunQa.Modules.Curriculum.Application;

public sealed class CurriculumConflictException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}
