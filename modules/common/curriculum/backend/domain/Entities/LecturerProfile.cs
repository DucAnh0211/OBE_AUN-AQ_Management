namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

/// <summary>Hồ sơ giảng viên gắn với tài khoản lecturer (UserId thuộc schema identity, không có FK chéo DbContext).</summary>
public sealed class LecturerProfile
{
    public long UserId { get; set; }
    public string LecturerCode { get; set; } = "";
    public long? DepartmentId { get; set; }
    public string? AcademicTitle { get; set; }
    public string? Degree { get; set; }
    public DateTime UpdatedAt { get; set; }
}
