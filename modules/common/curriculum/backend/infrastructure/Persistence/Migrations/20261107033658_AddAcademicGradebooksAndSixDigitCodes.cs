using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Curriculum.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicGradebooksAndSixDigitCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GradeSchemeRevision",
                schema: "curriculum",
                table: "program_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LegacyCourseCodeSnapshot",
                schema: "curriculum",
                table: "program_courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacyInstitutionalCode",
                schema: "curriculum",
                table: "courses",
                type: "text",
                nullable: true);

            // One-time, transactional data migration; preserve provenance before assigning internal codes.
            migrationBuilder.Sql("""
                DO $migration$
                DECLARE c record; next_code integer; current_university bigint := -1; new_code text;
                BEGIN
                  FOR c IN SELECT id, university_id, institutional_code FROM curriculum.courses
                           WHERE institutional_code IS NULL OR institutional_code !~ '^[0-9]{6}$'
                           ORDER BY university_id, id
                  LOOP
                    IF current_university <> c.university_id THEN next_code := 1; current_university := c.university_id; END IF;
                    LOOP
                      IF next_code > 999999 THEN RAISE EXCEPTION 'No available six-digit course code'; END IF;
                      new_code := lpad(next_code::text, 6, '0');
                      next_code := next_code + 1;
                      EXIT WHEN NOT EXISTS (SELECT 1 FROM curriculum.courses WHERE university_id = c.university_id AND institutional_code = new_code);
                    END LOOP;
                    UPDATE curriculum.courses SET "LegacyInstitutionalCode" = institutional_code, institutional_code = new_code WHERE id = c.id;
                  END LOOP;
                  UPDATE curriculum.program_courses pc SET "LegacyCourseCodeSnapshot" = pc.course_code_snapshot,
                    course_code_snapshot = stored_course.institutional_code
                  FROM curriculum.courses stored_course WHERE stored_course.id = pc.course_id AND pc.course_code_snapshot IS DISTINCT FROM stored_course.institutional_code;
                END $migration$;
                """);
            migrationBuilder.AlterColumn<string>(name: "institutional_code", schema: "curriculum", table: "courses",
                type: "character varying(6)", maxLength: 6, nullable: false, oldClrType: typeof(string), oldType: "text", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "course_code_snapshot", schema: "curriculum", table: "program_courses",
                type: "character varying(6)", maxLength: 6, nullable: false, oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.CreateTable(
                name: "academic_students",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniversityId = table.Column<long>(type: "bigint", nullable: false),
                    StudentCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_academic_students_universities_UniversityId",
                        column: x => x.UniversityId,
                        principalSchema: "curriculum",
                        principalTable: "universities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assessment_components",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramVersionId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_components", x => x.Id);
                    table.CheckConstraint("assessment_max_score_positive", "\"MaxScore\" > 0 AND \"MaxScore\" <= 1000");
                    table.CheckConstraint("assessment_weight_positive", "\"Weight\" > 0 AND \"Weight\" <= 100");
                    table.ForeignKey(
                        name: "FK_assessment_components_program_versions_ProgramVersionId",
                        column: x => x.ProgramVersionId,
                        principalSchema: "curriculum",
                        principalTable: "program_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "course_enrollments",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramCourseId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    AcademicPeriod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_course_enrollments_academic_students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "curriculum",
                        principalTable: "academic_students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_course_enrollments_program_courses_ProgramCourseId",
                        column: x => x.ProgramCourseId,
                        principalSchema: "curriculum",
                        principalTable: "program_courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grade_changes",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnrollmentId = table.Column<long>(type: "bigint", nullable: false),
                    ComponentId = table.Column<long>(type: "bigint", nullable: false),
                    PreviousScore = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    NewScore = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    ActorId = table.Column<long>(type: "bigint", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grade_changes_assessment_components_ComponentId",
                        column: x => x.ComponentId,
                        principalSchema: "curriculum",
                        principalTable: "assessment_components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grade_changes_course_enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalSchema: "curriculum",
                        principalTable: "course_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_grades",
                schema: "curriculum",
                columns: table => new
                {
                    EnrollmentId = table.Column<long>(type: "bigint", nullable: false),
                    ComponentId = table.Column<long>(type: "bigint", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_grades", x => new { x.EnrollmentId, x.ComponentId });
                    table.CheckConstraint("student_grade_nonnegative", "\"Score\" >= 0");
                    table.ForeignKey(
                        name: "FK_student_grades_assessment_components_ComponentId",
                        column: x => x.ComponentId,
                        principalSchema: "curriculum",
                        principalTable: "assessment_components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_grades_course_enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalSchema: "curriculum",
                        principalTable: "course_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "course_snapshot_code_six_digits",
                schema: "curriculum",
                table: "program_courses",
                sql: "course_code_snapshot ~ '^[0-9]{6}$'");

            migrationBuilder.AddCheckConstraint(
                name: "course_code_six_digits",
                schema: "curriculum",
                table: "courses",
                sql: "institutional_code ~ '^[0-9]{6}$'");

            migrationBuilder.CreateIndex(
                name: "IX_academic_students_UniversityId_StudentCode",
                schema: "curriculum",
                table: "academic_students",
                columns: new[] { "UniversityId", "StudentCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assessment_components_ProgramVersionId_Code",
                schema: "curriculum",
                table: "assessment_components",
                columns: new[] { "ProgramVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_ProgramCourseId_AcademicPeriod_StudentId",
                schema: "curriculum",
                table: "course_enrollments",
                columns: new[] { "ProgramCourseId", "AcademicPeriod", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_StudentId",
                schema: "curriculum",
                table: "course_enrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_grade_changes_ComponentId",
                schema: "curriculum",
                table: "grade_changes",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_grade_changes_EnrollmentId_ChangedAt",
                schema: "curriculum",
                table: "grade_changes",
                columns: new[] { "EnrollmentId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_student_grades_ComponentId",
                schema: "curriculum",
                table: "student_grades",
                column: "ComponentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grade_changes",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "student_grades",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "assessment_components",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "course_enrollments",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "academic_students",
                schema: "curriculum");

            migrationBuilder.DropCheckConstraint(
                name: "course_snapshot_code_six_digits",
                schema: "curriculum",
                table: "program_courses");

            migrationBuilder.DropCheckConstraint(
                name: "course_code_six_digits",
                schema: "curriculum",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "GradeSchemeRevision",
                schema: "curriculum",
                table: "program_versions");

            migrationBuilder.DropColumn(
                name: "LegacyCourseCodeSnapshot",
                schema: "curriculum",
                table: "program_courses");

            migrationBuilder.DropColumn(
                name: "LegacyInstitutionalCode",
                schema: "curriculum",
                table: "courses");

            migrationBuilder.AlterColumn<string>(
                name: "course_code_snapshot",
                schema: "curriculum",
                table: "program_courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "institutional_code",
                schema: "curriculum",
                table: "courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6);
        }
    }
}
