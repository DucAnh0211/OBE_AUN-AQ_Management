using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

/// <summary>Quy tắc ghi danh chung cho nhập tay, import và thêm cả lớp sinh hoạt vào lớp học phần.</summary>
internal static class EnrollmentWriter
{
    public static async Task<string?> BlockerAsync(CurriculumDbContext db, CourseOffering offering, AcademicStudent student, CancellationToken ct)
    {
        if (student.Status != AcademicCodes.Studying) return "Sinh viên không ở trạng thái đang học (đang bảo lưu, thôi học hoặc đã tốt nghiệp).";
        if (await db.Set<CourseEnrollment>().AnyAsync(e => e.StudentId == student.Id && e.ProgramCourseId == offering.ProgramCourseId
                && e.AcademicPeriod == offering.AcademicPeriod && e.OfferingId != offering.Id, ct))
            return "Sinh viên đã học học phần này ở một lớp học phần khác trong cùng học kỳ.";
        return null;
    }
    public static async Task<bool> OverCapacityAsync(CurriculumDbContext db, CourseOffering offering, int adding, CancellationToken ct) =>
        offering.Capacity is int capacity && await db.Set<CourseEnrollment>().CountAsync(e => e.OfferingId == offering.Id, ct) + adding > capacity;
    /// <summary>Lần học trước của cùng học phần (theo mã học phần của trường, kể cả ở phiên bản CTĐT khác).</summary>
    public static async Task<string> AttemptKindAsync(CurriculumDbContext db, long studentId, CourseOffering offering, CancellationToken ct)
    {
        var courseId = await db.ProgramCourses.Where(c => c.Id == offering.ProgramCourseId).Select(c => c.CourseId).SingleAsync(ct);
        var earlier = await (from e in db.Set<CourseEnrollment>() join c in db.ProgramCourses on e.ProgramCourseId equals c.Id
            where e.StudentId == studentId && c.CourseId == courseId && string.Compare(e.AcademicPeriod, offering.AcademicPeriod) < 0
            select e.Passed).ToArrayAsync(ct);
        return GradeRules.AttemptKind(earlier);
    }
    public static async Task<CourseEnrollment> AddAsync(CurriculumDbContext db, CourseOffering offering, AcademicStudent student, CancellationToken ct)
    {
        var row = new CourseEnrollment { OfferingId = offering.Id, ProgramCourseId = offering.ProgramCourseId, StudentId = student.Id,
            AcademicPeriod = offering.AcademicPeriod, AttemptKind = await AttemptKindAsync(db, student.Id, offering, ct), CreatedAt = DateTime.UtcNow };
        db.Add(row);
        return row;
    }
}
