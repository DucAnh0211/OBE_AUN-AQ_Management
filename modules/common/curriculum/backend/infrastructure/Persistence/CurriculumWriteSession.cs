using Microsoft.EntityFrameworkCore;
using Npgsql;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class CurriculumWriteSession(CurriculumDbContext db)
{
    // PostgreSQL row locks preserve the existing edit-versus-publish guarantee.
    // EF Core does not expose FOR UPDATE in LINQ; all business CRUD uses tracked entities.
    public async Task<ProgramVersion> LockVersionAsync(long id, CancellationToken ct)
    {
        var rows = await db.ProgramVersions.FromSqlInterpolated(
            $"SELECT * FROM curriculum.program_versions WHERE id = {id} AND archived_at IS NULL FOR UPDATE").ToListAsync(ct);
        return rows.SingleOrDefault() ?? throw new CurriculumNotFoundException("phiên bản chương trình đào tạo", id);
    }
    public async Task<ProgramVersion> DraftAsync(long id, CancellationToken ct)
    {
        var version = await LockVersionAsync(id, ct);
        if (!VersionRules.CanEdit(version.DatasetStatus, version.ArchivedAt))
            throw new CurriculumConflictException("version_not_editable", "Chỉ phiên bản ở trạng thái draft mới được chỉnh sửa.");
        return version;
    }
    public async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await action();
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw Conflict((PostgresException)ex.InnerException!);
        }
    }
    private static CurriculumConflictException Conflict(PostgresException pg) => pg.ConstraintName switch
    {
        "universities_code_key" => new("university_code_exists", "Mã trường đã tồn tại."),
        "programs_university_id_code_key" => new("program_code_exists", "Mã chương trình đào tạo đã tồn tại trong trường này."),
        "program_versions_program_id_version_code_key" => new("program_version_code_exists", "Mã phiên bản đã tồn tại trong chương trình đào tạo."),
        "program_courses_active_course_uq" => new("course_already_in_version", "Học phần đã tồn tại trong phiên bản chương trình đào tạo."),
        "plos_program_version_id_code_key" => new("plo_code_exists", "Mã PLO đã tồn tại trong phiên bản."),
        "clos_program_course_id_code_key" => new("clo_code_exists", "Mã CLO đã tồn tại trong học phần."),
        "course_plos_program_course_id_plo_id_key" => new("course_plo_mapping_exists", "Học phần đã được ánh xạ với PLO này."),
        "clo_plos_pkey" => new("clo_plo_mapping_exists", "CLO đã được ánh xạ với PLO này."),
        "IX_org_units_UniversityId_Code" => new("org_unit_code_exists", "Mã khoa/bộ môn đã tồn tại trong trường."),
        "IX_home_classes_ProgramVersionId_Code" => new("home_class_code_exists", "Mã lớp sinh hoạt đã tồn tại trong phiên bản CTĐT."),
        "IX_lecturer_profiles_LecturerCode" => new("lecturer_code_exists", "Mã giảng viên đã được dùng cho giảng viên khác."),
        "IX_course_offerings_ProgramCourseId_AcademicPeriod_GroupName" => new("offering_group_exists", "Học phần đã có nhóm lớp này trong học kỳ."),
        "IX_academic_students_UserId" => new("student_account_linked", "Tài khoản này đã được liên kết với hồ sơ sinh viên khác."),
        "IX_academic_students_UniversityId_StudentCode" => new("student_code_exists", "Mã sinh viên đã tồn tại trong trường."),
        "IX_grade_scales_UniversityId_Letter" => new("grade_letter_exists", "Điểm chữ bị trùng trong thang điểm."),
        "offering_lecturers_one_main" => new("offering_main_lecturer", "Lớp học phần chỉ có một giảng viên chính."),
        _ => new("course_code_exists", "Dữ liệu trùng mã hoặc liên kết đã tồn tại.")
    };
}
