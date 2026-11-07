namespace ObeAunQa.Modules.Curriculum.Application;

public sealed class AcademicAccessException : Exception
{
    public AcademicAccessException() : base("Bạn không có quyền xem hoặc thay đổi sinh viên và điểm của học phần này.") { }
}
