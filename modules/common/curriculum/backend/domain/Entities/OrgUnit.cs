namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

/// <summary>Đơn vị tổ chức: khoa (faculty) hoặc bộ môn (department, thuộc một khoa).</summary>
public sealed class OrgUnit
{
    public long Id { get; set; }
    public long UniversityId { get; set; }
    public long? ParentId { get; set; }
    public string Kind { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}
