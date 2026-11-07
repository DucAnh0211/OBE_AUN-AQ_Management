using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Curriculum.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSemesterClassesAndCourseOfferings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_course_enrollments_ProgramCourseId_AcademicPeriod_StudentId",
                schema: "curriculum",
                table: "course_enrollments");

            migrationBuilder.CreateSequence(
                name: "course_offering_codes",
                schema: "curriculum",
                startValue: 100000L,
                maxValue: 999999L);

            migrationBuilder.AddColumn<long>(
                name: "OfferingId",
                schema: "curriculum",
                table: "course_enrollments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "academic_classes",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramVersionId = table.Column<long>(type: "bigint", nullable: false),
                    AcademicPeriod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_classes", x => x.Id);
                    table.UniqueConstraint("AK_academic_classes_Id_ProgramVersionId_AcademicPeriod", x => new { x.Id, x.ProgramVersionId, x.AcademicPeriod });
                    table.ForeignKey(
                        name: "FK_academic_classes_program_versions_ProgramVersionId",
                        column: x => x.ProgramVersionId,
                        principalSchema: "curriculum",
                        principalTable: "program_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "course_offerings",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicClassId = table.Column<long>(type: "bigint", nullable: false),
                    ProgramVersionId = table.Column<long>(type: "bigint", nullable: false),
                    ProgramCourseId = table.Column<long>(type: "bigint", nullable: false),
                    AcademicPeriod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, defaultValueSql: "lpad(nextval('curriculum.course_offering_codes')::text, 6, '0')"),
                    LecturerId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_offerings", x => x.Id);
                    table.UniqueConstraint("AK_course_offerings_Id_ProgramCourseId_AcademicPeriod", x => new { x.Id, x.ProgramCourseId, x.AcademicPeriod });
                    table.CheckConstraint("offering_code_six_digits", "\"Code\" ~ '^[0-9]{6}$'");
                    table.ForeignKey(
                        name: "FK_course_offerings_academic_classes_AcademicClassId_ProgramVe~",
                        columns: x => new { x.AcademicClassId, x.ProgramVersionId, x.AcademicPeriod },
                        principalSchema: "curriculum",
                        principalTable: "academic_classes",
                        principalColumns: new[] { "Id", "ProgramVersionId", "AcademicPeriod" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_course_offerings_program_courses_ProgramCourseId_ProgramVer~",
                        columns: x => new { x.ProgramCourseId, x.ProgramVersionId },
                        principalSchema: "curriculum",
                        principalTable: "program_courses",
                        principalColumns: new[] { "id", "program_version_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            // Historical gradebooks have no class label: preserve their IDs and scores in an explicit legacy class.
            migrationBuilder.Sql("""
                INSERT INTO curriculum.academic_classes ("ProgramVersionId", "AcademicPeriod", "Code", "Name", "CreatedAt")
                SELECT DISTINCT c.program_version_id, e."AcademicPeriod", 'LEGACY', 'Dữ liệu cũ', now()
                FROM curriculum.course_enrollments e JOIN curriculum.program_courses c ON c.id = e."ProgramCourseId";

                INSERT INTO curriculum.course_offerings ("AcademicClassId", "ProgramVersionId", "ProgramCourseId", "AcademicPeriod", "CreatedAt")
                SELECT DISTINCT a."Id", c.program_version_id, c.id, e."AcademicPeriod", now()
                FROM curriculum.course_enrollments e JOIN curriculum.program_courses c ON c.id = e."ProgramCourseId"
                JOIN curriculum.academic_classes a ON a."ProgramVersionId" = c.program_version_id AND a."AcademicPeriod" = e."AcademicPeriod";

                UPDATE curriculum.course_enrollments e SET "OfferingId" = o."Id"
                FROM curriculum.course_offerings o WHERE o."ProgramCourseId" = e."ProgramCourseId" AND o."AcademicPeriod" = e."AcademicPeriod";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_OfferingId_ProgramCourseId_AcademicPeriod",
                schema: "curriculum",
                table: "course_enrollments",
                columns: new[] { "OfferingId", "ProgramCourseId", "AcademicPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_OfferingId_StudentId",
                schema: "curriculum",
                table: "course_enrollments",
                columns: new[] { "OfferingId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_ProgramCourseId",
                schema: "curriculum",
                table: "course_enrollments",
                column: "ProgramCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_academic_classes_ProgramVersionId_AcademicPeriod_Code",
                schema: "curriculum",
                table: "academic_classes",
                columns: new[] { "ProgramVersionId", "AcademicPeriod", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_offerings_AcademicClassId_ProgramCourseId",
                schema: "curriculum",
                table: "course_offerings",
                columns: new[] { "AcademicClassId", "ProgramCourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_offerings_AcademicClassId_ProgramVersionId_AcademicP~",
                schema: "curriculum",
                table: "course_offerings",
                columns: new[] { "AcademicClassId", "ProgramVersionId", "AcademicPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_course_offerings_Code",
                schema: "curriculum",
                table: "course_offerings",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_offerings_LecturerId",
                schema: "curriculum",
                table: "course_offerings",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_course_offerings_ProgramCourseId_ProgramVersionId",
                schema: "curriculum",
                table: "course_offerings",
                columns: new[] { "ProgramCourseId", "ProgramVersionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_course_enrollments_course_offerings_OfferingId_ProgramCours~",
                schema: "curriculum",
                table: "course_enrollments",
                columns: new[] { "OfferingId", "ProgramCourseId", "AcademicPeriod" },
                principalSchema: "curriculum",
                principalTable: "course_offerings",
                principalColumns: new[] { "Id", "ProgramCourseId", "AcademicPeriod" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_enrollments_course_offerings_OfferingId_ProgramCours~",
                schema: "curriculum",
                table: "course_enrollments");

            migrationBuilder.DropTable(
                name: "course_offerings",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "academic_classes",
                schema: "curriculum");

            migrationBuilder.DropIndex(
                name: "IX_course_enrollments_OfferingId_ProgramCourseId_AcademicPeriod",
                schema: "curriculum",
                table: "course_enrollments");

            migrationBuilder.DropIndex(
                name: "IX_course_enrollments_OfferingId_StudentId",
                schema: "curriculum",
                table: "course_enrollments");

            migrationBuilder.DropIndex(
                name: "IX_course_enrollments_ProgramCourseId",
                schema: "curriculum",
                table: "course_enrollments");

            migrationBuilder.DropColumn(
                name: "OfferingId",
                schema: "curriculum",
                table: "course_enrollments");

            migrationBuilder.DropSequence(
                name: "course_offering_codes",
                schema: "curriculum");

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_ProgramCourseId_AcademicPeriod_StudentId",
                schema: "curriculum",
                table: "course_enrollments",
                columns: new[] { "ProgramCourseId", "AcademicPeriod", "StudentId" },
                unique: true);
        }
    }
}
