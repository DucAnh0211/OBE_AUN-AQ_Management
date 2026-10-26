namespace ObeAunQa.Modules.Curriculum.Application;

public sealed class CurriculumNotFoundException(string resource, long id)
    : Exception($"Không tìm thấy {resource} có mã định danh {id}.")
{
    public string Resource { get; } = resource;

    public long ResourceId { get; } = id;
}
