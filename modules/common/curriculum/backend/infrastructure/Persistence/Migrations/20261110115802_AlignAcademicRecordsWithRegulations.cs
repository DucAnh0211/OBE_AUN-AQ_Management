using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Curriculum.Infrastructure.Migrations
{
    /// <summary>
    /// Đưa mô hình đào tạo về sát thực tế (Thông tư 08/2021/TT-BGDĐT): khoa/bộ môn, hồ sơ giảng viên, lớp sinh hoạt,
    /// lớp học phần độc lập với nhiều giảng viên, cơ cấu điểm theo đề cương học phần + liên kết CLO,
    /// quy chế/thang điểm chữ, quy trình nộp–công bố điểm, học lại/cải thiện.
    /// Thứ tự viết tay: (1) thêm cấu trúc mới, (2) chuyển dữ liệu bằng SQL giữ nguyên ID điểm và lịch sử, (3) xóa cấu trúc cũ.
    /// </summary>
    public partial class AlignAcademicRecordsWithRegulations : Migration
    {
        private const string S = "curriculum";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- (1) Cấu trúc mới ----------
            migrationBuilder.CreateTable(
                name: "org_units", schema: S,
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniversityId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_units", x => x.Id);
                    table.CheckConstraint("org_unit_kind", "\"Kind\" IN ('faculty', 'department')");
                    table.ForeignKey("FK_org_units_org_units_ParentId", x => x.ParentId, principalSchema: S, principalTable: "org_units", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_org_units_universities_UniversityId", x => x.UniversityId, principalSchema: S, principalTable: "universities", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lecturer_profiles", schema: S,
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    LecturerCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DepartmentId = table.Column<long>(type: "bigint", nullable: true),
                    AcademicTitle = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Degree = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lecturer_profiles", x => x.UserId);
                    table.ForeignKey("FK_lecturer_profiles_org_units_DepartmentId", x => x.DepartmentId, principalSchema: S, principalTable: "org_units", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "home_classes", schema: S,
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramVersionId = table.Column<long>(type: "bigint", nullable: false),
                    CohortCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AdvisorUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_home_classes", x => x.Id);
                    table.ForeignKey("FK_home_classes_program_versions_ProgramVersionId", x => x.ProgramVersionId, principalSchema: S, principalTable: "program_versions", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "offering_lecturers", schema: S,
                columns: table => new
                {
                    OfferingId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offering_lecturers", x => new { x.OfferingId, x.UserId });
                    table.CheckConstraint("offering_lecturer_role", "\"Role\" IN ('main', 'assistant', 'practice')");
                    table.ForeignKey("FK_offering_lecturers_course_offerings_OfferingId", x => x.OfferingId, principalSchema: S, principalTable: "course_offerings", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grading_policies", schema: S,
                columns: table => new
                {
                    UniversityId = table.Column<long>(type: "bigint", nullable: false),
                    CourseScoreDecimals = table.Column<int>(type: "integer", nullable: false),
                    MinComponents = table.Column<int>(type: "integer", nullable: false),
                    MinFinalWeight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    RetakePolicy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GpaDecimals = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grading_policies", x => x.UniversityId);
                    table.CheckConstraint("grading_policy_components", "\"MinComponents\" BETWEEN 1 AND 10");
                    table.CheckConstraint("grading_policy_decimals", "\"CourseScoreDecimals\" BETWEEN 0 AND 2 AND \"GpaDecimals\" BETWEEN 1 AND 3");
                    table.CheckConstraint("grading_policy_final_weight", "\"MinFinalWeight\" IS NULL OR (\"MinFinalWeight\" > 0 AND \"MinFinalWeight\" <= 100)");
                    table.CheckConstraint("grading_policy_retake", "\"RetakePolicy\" IN ('highest', 'latest')");
                    table.ForeignKey("FK_grading_policies_universities_UniversityId", x => x.UniversityId, principalSchema: S, principalTable: "universities", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grade_scales", schema: S,
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniversityId = table.Column<long>(type: "bigint", nullable: false),
                    Letter = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    MinScore = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Point4 = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    IsPassing = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_scales", x => x.Id);
                    table.CheckConstraint("grade_scale_min_score", "\"MinScore\" >= 0 AND \"MinScore\" <= 10");
                    table.CheckConstraint("grade_scale_point4", "\"Point4\" >= 0 AND \"Point4\" <= 4");
                    table.ForeignKey("FK_grade_scales_universities_UniversityId", x => x.UniversityId, principalSchema: S, principalTable: "universities", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<long>(name: "faculty_id", schema: S, table: "programs", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(name: "department_id", schema: S, table: "courses", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "counts_toward_gpa", schema: S, table: "program_courses", type: "boolean", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<int>(name: "grade_scheme_revision", schema: S, table: "program_courses", type: "integer", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "Reason", schema: S, table: "grade_changes", type: "character varying(500)", maxLength: 500, nullable: true);

            migrationBuilder.AddColumn<long>(name: "TargetHomeClassId", schema: S, table: "course_offerings", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "GroupName", schema: S, table: "course_offerings", type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "N01");
            migrationBuilder.AddColumn<int>(name: "Capacity", schema: S, table: "course_offerings", type: "integer", nullable: true);
            migrationBuilder.AddColumn<string>(name: "GradeStatus", schema: S, table: "course_offerings", type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "draft");
            migrationBuilder.AddColumn<string>(name: "StatusNote", schema: S, table: "course_offerings", type: "character varying(500)", maxLength: 500, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "SubmittedAt", schema: S, table: "course_offerings", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<long>(name: "SubmittedBy", schema: S, table: "course_offerings", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "PublishedAt", schema: S, table: "course_offerings", type: "timestamp with time zone", nullable: true);
            migrationBuilder.AddColumn<long>(name: "PublishedBy", schema: S, table: "course_offerings", type: "bigint", nullable: true);

            migrationBuilder.AddColumn<string>(name: "AttemptKind", schema: S, table: "course_enrollments", type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "first");
            migrationBuilder.AddColumn<string>(name: "SpecialMark", schema: S, table: "course_enrollments", type: "character varying(2)", maxLength: 2, nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "FinalScore", schema: S, table: "course_enrollments", type: "numeric(5,2)", precision: 5, scale: 2, nullable: true);
            migrationBuilder.AddColumn<string>(name: "LetterGrade", schema: S, table: "course_enrollments", type: "character varying(5)", maxLength: 5, nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "GradePoint", schema: S, table: "course_enrollments", type: "numeric(3,2)", precision: 3, scale: 2, nullable: true);
            migrationBuilder.AddColumn<bool>(name: "Passed", schema: S, table: "course_enrollments", type: "boolean", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "PublishedAt", schema: S, table: "course_enrollments", type: "timestamp with time zone", nullable: true);

            migrationBuilder.AddColumn<long>(name: "ProgramCourseId", schema: S, table: "assessment_components", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Kind", schema: S, table: "assessment_components", type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "process");

            migrationBuilder.AddColumn<long>(name: "HomeClassId", schema: S, table: "academic_students", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(name: "ProgramVersionId", schema: S, table: "academic_students", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "CohortCode", schema: S, table: "academic_students", type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Status", schema: S, table: "academic_students", type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "studying");
            migrationBuilder.AddColumn<long>(name: "UserId", schema: S, table: "academic_students", type: "bigint", nullable: true);

            migrationBuilder.CreateTable(
                name: "assessment_component_clos", schema: S,
                columns: table => new
                {
                    ComponentId = table.Column<long>(type: "bigint", nullable: false),
                    CloId = table.Column<long>(type: "bigint", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_component_clos", x => new { x.ComponentId, x.CloId });
                    table.CheckConstraint("assessment_clo_weight", "\"Weight\" > 0 AND \"Weight\" <= 100");
                    table.ForeignKey("FK_assessment_component_clos_assessment_components_ComponentId", x => x.ComponentId, principalSchema: S, principalTable: "assessment_components", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_assessment_component_clos_clos_CloId", x => x.CloId, principalSchema: S, principalTable: "clos", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            // Mã đầu điểm từ nay chỉ duy nhất trong một học phần; bỏ ràng buộc cấp phiên bản trước khi sao chép.
            migrationBuilder.DropIndex(name: "IX_assessment_components_ProgramVersionId_Code", schema: S, table: "assessment_components");

            // ---------- (2) Chuyển dữ liệu ----------
            migrationBuilder.Sql("""
                -- Giảng viên đang phân công cho lớp học phần trở thành giảng viên chính.
                INSERT INTO curriculum.offering_lecturers ("OfferingId", "UserId", "Role", "AssignedAt")
                SELECT o."Id", o."LecturerId", 'main', now() FROM curriculum.course_offerings o WHERE o."LecturerId" IS NOT NULL;

                -- "Lớp theo học kỳ" cũ → lớp sinh hoạt cố định: gộp theo (phiên bản, mã lớp); lớp LEGACY không phải lớp thật.
                INSERT INTO curriculum.home_classes ("ProgramVersionId", "Code", "Name", "CreatedAt")
                SELECT a."ProgramVersionId", a."Code", (array_agg(a."Name" ORDER BY a."Id" DESC))[1], min(a."CreatedAt")
                FROM curriculum.academic_classes a WHERE a."Code" <> 'LEGACY' GROUP BY a."ProgramVersionId", a."Code";

                UPDATE curriculum.course_offerings o SET "TargetHomeClassId" = h."Id"
                FROM curriculum.academic_classes a JOIN curriculum.home_classes h ON h."ProgramVersionId" = a."ProgramVersionId" AND h."Code" = a."Code"
                WHERE a."Id" = o."AcademicClassId";

                -- Nhóm lớp N01, N02… cho các lớp học phần cùng học phần trong cùng học kỳ.
                UPDATE curriculum.course_offerings o SET "GroupName" = 'N' || lpad(r.n::text, 2, '0')
                FROM (SELECT "Id", row_number() OVER (PARTITION BY "ProgramCourseId", "AcademicPeriod" ORDER BY "Id") AS n FROM curriculum.course_offerings) r
                WHERE r."Id" = o."Id";

                -- Sinh viên chỉ học ở đúng một lớp sinh hoạt thì gán lớp đó; trường hợp nhiều lớp để admin xử lý.
                WITH c AS (
                    SELECT e."StudentId", min(o."TargetHomeClassId") AS hc, count(DISTINCT o."TargetHomeClassId") AS n
                    FROM curriculum.course_enrollments e JOIN curriculum.course_offerings o ON o."Id" = e."OfferingId"
                    WHERE o."TargetHomeClassId" IS NOT NULL GROUP BY e."StudentId")
                UPDATE curriculum.academic_students s SET "HomeClassId" = c.hc, "ProgramVersionId" = h."ProgramVersionId"
                FROM c JOIN curriculum.home_classes h ON h."Id" = c.hc WHERE c."StudentId" = s."Id" AND c.n = 1;

                -- Cơ cấu điểm chung của phiên bản → đề cương từng học phần (kể cả học phần đã lưu trữ để giữ điểm cũ).
                INSERT INTO curriculum.assessment_components ("ProgramVersionId", "ProgramCourseId", "Code", "Name", "Kind", "Weight", "MaxScore", "DisplayOrder")
                SELECT a."ProgramVersionId", pc.id, a."Code", a."Name",
                    CASE WHEN a."Code" IN ('THI', 'CK', 'CUOIKY', 'FINAL') OR a."Name" ILIKE '%cuối%' OR a."Name" ILIKE '%thi kết thúc%' THEN 'final'
                         WHEN a."Code" IN ('GK', 'GIUAKY', 'MID') OR a."Name" ILIKE '%giữa%' THEN 'midterm'
                         WHEN a."Code" IN ('CC', 'DIEMDANH') OR a."Name" ILIKE '%chuyên cần%' THEN 'attendance'
                         ELSE 'process' END,
                    a."Weight", a."MaxScore", a."DisplayOrder"
                FROM curriculum.assessment_components a JOIN curriculum.program_courses pc ON pc.program_version_id = a."ProgramVersionId"
                WHERE a."ProgramCourseId" IS NULL;

                UPDATE curriculum.student_grades g SET "ComponentId" = n."Id"
                FROM curriculum.course_enrollments e, curriculum.assessment_components old, curriculum.assessment_components n
                WHERE g."EnrollmentId" = e."Id" AND old."Id" = g."ComponentId" AND old."ProgramCourseId" IS NULL
                  AND n."ProgramCourseId" = e."ProgramCourseId" AND n."Code" = old."Code";

                UPDATE curriculum.grade_changes g SET "ComponentId" = n."Id"
                FROM curriculum.course_enrollments e, curriculum.assessment_components old, curriculum.assessment_components n
                WHERE g."EnrollmentId" = e."Id" AND old."Id" = g."ComponentId" AND old."ProgramCourseId" IS NULL
                  AND n."ProgramCourseId" = e."ProgramCourseId" AND n."Code" = old."Code";

                UPDATE curriculum.program_courses pc SET grade_scheme_revision = v."GradeSchemeRevision"
                FROM curriculum.program_versions v
                WHERE v.id = pc.program_version_id AND EXISTS (SELECT 1 FROM curriculum.assessment_components a WHERE a."ProgramCourseId" = pc.id);

                DELETE FROM curriculum.assessment_components WHERE "ProgramCourseId" IS NULL;

                -- Quy chế mặc định theo Thông tư 08/2021/TT-BGDĐT cho từng trường.
                INSERT INTO curriculum.grading_policies ("UniversityId", "CourseScoreDecimals", "MinComponents", "MinFinalWeight", "RetakePolicy", "GpaDecimals", "UpdatedAt")
                SELECT u.id, 1, 2, NULL, 'highest', 2, now() FROM curriculum.universities u;

                INSERT INTO curriculum.grade_scales ("UniversityId", "Letter", "MinScore", "Point4", "IsPassing", "DisplayOrder")
                SELECT u.id, b.letter, b.min_score, b.point4, b.passing, b.ord
                FROM curriculum.universities u CROSS JOIN (VALUES ('A', 8.5, 4, true, 1), ('B', 7.0, 3, true, 2), ('C', 5.5, 2, true, 3),
                    ('D', 4.0, 1, true, 4), ('F', 0.0, 0, false, 5)) AS b(letter, min_score, point4, passing, ord);
                """);

            // ---------- (3) Bỏ cấu trúc cũ, bật ràng buộc mới ----------
            migrationBuilder.DropForeignKey(name: "FK_assessment_components_program_versions_ProgramVersionId", schema: S, table: "assessment_components");
            migrationBuilder.DropColumn(name: "ProgramVersionId", schema: S, table: "assessment_components");
            migrationBuilder.AlterColumn<long>(name: "ProgramCourseId", schema: S, table: "assessment_components", type: "bigint", nullable: false, oldClrType: typeof(long), oldType: "bigint", oldNullable: true);

            migrationBuilder.DropForeignKey(name: "FK_course_offerings_academic_classes_AcademicClassId_ProgramVe~", schema: S, table: "course_offerings");
            migrationBuilder.DropIndex(name: "IX_course_offerings_AcademicClassId_ProgramCourseId", schema: S, table: "course_offerings");
            migrationBuilder.DropIndex(name: "IX_course_offerings_AcademicClassId_ProgramVersionId_AcademicP~", schema: S, table: "course_offerings");
            migrationBuilder.DropIndex(name: "IX_course_offerings_LecturerId", schema: S, table: "course_offerings");
            migrationBuilder.DropColumn(name: "AcademicClassId", schema: S, table: "course_offerings");
            migrationBuilder.DropColumn(name: "LecturerId", schema: S, table: "course_offerings");
            migrationBuilder.DropTable(name: "academic_classes", schema: S);
            migrationBuilder.DropColumn(name: "GradeSchemeRevision", schema: S, table: "program_versions");
            migrationBuilder.DropIndex(name: "IX_course_enrollments_StudentId", schema: S, table: "course_enrollments");

            migrationBuilder.CreateIndex(name: "IX_assessment_components_ProgramCourseId_Code", schema: S, table: "assessment_components", columns: new[] { "ProgramCourseId", "Code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_programs_faculty_id", schema: S, table: "programs", column: "faculty_id");
            migrationBuilder.CreateIndex(name: "IX_courses_department_id", schema: S, table: "courses", column: "department_id");
            migrationBuilder.CreateIndex(name: "IX_course_offerings_ProgramCourseId_AcademicPeriod_GroupName", schema: S, table: "course_offerings", columns: new[] { "ProgramCourseId", "AcademicPeriod", "GroupName" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_course_offerings_ProgramVersionId_AcademicPeriod", schema: S, table: "course_offerings", columns: new[] { "ProgramVersionId", "AcademicPeriod" });
            migrationBuilder.CreateIndex(name: "IX_course_offerings_TargetHomeClassId", schema: S, table: "course_offerings", column: "TargetHomeClassId");
            migrationBuilder.CreateIndex(name: "IX_course_enrollments_StudentId_AcademicPeriod", schema: S, table: "course_enrollments", columns: new[] { "StudentId", "AcademicPeriod" });
            migrationBuilder.CreateIndex(name: "IX_academic_students_HomeClassId", schema: S, table: "academic_students", column: "HomeClassId");
            migrationBuilder.CreateIndex(name: "IX_academic_students_ProgramVersionId", schema: S, table: "academic_students", column: "ProgramVersionId");
            migrationBuilder.CreateIndex(name: "IX_academic_students_UserId", schema: S, table: "academic_students", column: "UserId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_assessment_component_clos_CloId", schema: S, table: "assessment_component_clos", column: "CloId");
            migrationBuilder.CreateIndex(name: "IX_grade_scales_UniversityId_Letter", schema: S, table: "grade_scales", columns: new[] { "UniversityId", "Letter" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_home_classes_AdvisorUserId", schema: S, table: "home_classes", column: "AdvisorUserId");
            migrationBuilder.CreateIndex(name: "IX_home_classes_ProgramVersionId_Code", schema: S, table: "home_classes", columns: new[] { "ProgramVersionId", "Code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_lecturer_profiles_DepartmentId", schema: S, table: "lecturer_profiles", column: "DepartmentId");
            migrationBuilder.CreateIndex(name: "IX_lecturer_profiles_LecturerCode", schema: S, table: "lecturer_profiles", column: "LecturerCode", unique: true);
            migrationBuilder.CreateIndex(name: "IX_offering_lecturers_UserId", schema: S, table: "offering_lecturers", column: "UserId");
            migrationBuilder.CreateIndex(name: "offering_lecturers_one_main", schema: S, table: "offering_lecturers", column: "OfferingId", unique: true, filter: "\"Role\" = 'main'");
            migrationBuilder.CreateIndex(name: "IX_org_units_ParentId", schema: S, table: "org_units", column: "ParentId");
            migrationBuilder.CreateIndex(name: "IX_org_units_UniversityId_Code", schema: S, table: "org_units", columns: new[] { "UniversityId", "Code" }, unique: true);

            migrationBuilder.AddCheckConstraint(name: "offering_capacity_range", schema: S, table: "course_offerings", sql: "\"Capacity\" IS NULL OR (\"Capacity\" >= 1 AND \"Capacity\" <= 1000)");
            migrationBuilder.AddCheckConstraint(name: "offering_grade_status", schema: S, table: "course_offerings", sql: "\"GradeStatus\" IN ('draft', 'submitted', 'published')");
            migrationBuilder.AddCheckConstraint(name: "enrollment_attempt_kind", schema: S, table: "course_enrollments", sql: "\"AttemptKind\" IN ('first', 'retake', 'improve')");
            migrationBuilder.AddCheckConstraint(name: "enrollment_special_mark", schema: S, table: "course_enrollments", sql: "\"SpecialMark\" IS NULL OR \"SpecialMark\" IN ('I', 'R', 'P')");
            migrationBuilder.AddCheckConstraint(name: "assessment_kind", schema: S, table: "assessment_components", sql: "\"Kind\" IN ('attendance', 'process', 'midterm', 'practice', 'project', 'final')");
            migrationBuilder.AddCheckConstraint(name: "academic_student_status", schema: S, table: "academic_students", sql: "\"Status\" IN ('studying', 'suspended', 'dropped', 'graduated')");

            migrationBuilder.AddForeignKey(name: "FK_academic_students_home_classes_HomeClassId", schema: S, table: "academic_students", column: "HomeClassId", principalSchema: S, principalTable: "home_classes", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_academic_students_program_versions_ProgramVersionId", schema: S, table: "academic_students", column: "ProgramVersionId", principalSchema: S, principalTable: "program_versions", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_assessment_components_program_courses_ProgramCourseId", schema: S, table: "assessment_components", column: "ProgramCourseId", principalSchema: S, principalTable: "program_courses", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_course_offerings_home_classes_TargetHomeClassId", schema: S, table: "course_offerings", column: "TargetHomeClassId", principalSchema: S, principalTable: "home_classes", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "courses_department_id_fkey", schema: S, table: "courses", column: "department_id", principalSchema: S, principalTable: "org_units", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "programs_faculty_id_fkey", schema: S, table: "programs", column: "faculty_id", principalSchema: S, principalTable: "org_units", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dữ liệu đã được gộp (lớp sinh hoạt, cơ cấu điểm theo học phần) nên không thể đảo ngược tự động mà không mất thông tin.
            throw new NotSupportedException("Khôi phục database từ bản sao lưu trước khi chạy migration AlignAcademicRecordsWithRegulations.");
        }
    }
}
