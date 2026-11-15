using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Curriculum.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lecturer_profiles_LecturerCode",
                schema: "curriculum",
                table: "lecturer_profiles");

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "student_grades",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "source_documents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "source_course_rows",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "programs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "program_versions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "program_courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "plos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "outcome_snapshots",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "outcome_policies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "org_units",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "org_unit_assignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "offering_lecturers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "lecturer_profiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "import_batches",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "home_classes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "grading_policies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "grade_scales",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "grade_changes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "course_plos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "course_offerings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "course_enrollments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "clos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "clo_plos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "audit_events",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "assessment_components",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "assessment_component_clos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "curriculum",
                table: "academic_students",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "grade_correction_requests",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferingId = table.Column<long>(type: "bigint", nullable: false),
                    EnrollmentId = table.Column<long>(type: "bigint", nullable: false),
                    RequestedBy = table.Column<long>(type: "bigint", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    BeforeJson = table.Column<string>(type: "jsonb", nullable: false),
                    ProposalJson = table.Column<string>(type: "jsonb", nullable: false),
                    ReviewedBy = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "text", nullable: true),
                    school_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_correction_requests", x => x.Id);
                    table.CheckConstraint("grade_correction_status_ck", "\"Status\" IN ('pending','approved','rejected')");
                    table.ForeignKey(
                        name: "FK_grade_correction_requests_course_enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalSchema: "curriculum",
                        principalTable: "course_enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grade_correction_requests_course_offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalSchema: "curriculum",
                        principalTable: "course_offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_student_grades_school_id",
                schema: "curriculum",
                table: "student_grades",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_source_documents_school_id",
                schema: "curriculum",
                table: "source_documents",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_source_course_rows_school_id",
                schema: "curriculum",
                table: "source_course_rows",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_programs_school_id",
                schema: "curriculum",
                table: "programs",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_program_versions_school_id",
                schema: "curriculum",
                table: "program_versions",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_program_courses_school_id",
                schema: "curriculum",
                table: "program_courses",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_plos_school_id",
                schema: "curriculum",
                table: "plos",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_outcome_snapshots_school_id",
                schema: "curriculum",
                table: "outcome_snapshots",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_outcome_policies_school_id",
                schema: "curriculum",
                table: "outcome_policies",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_org_units_school_id",
                schema: "curriculum",
                table: "org_units",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_org_unit_assignments_school_id",
                schema: "curriculum",
                table: "org_unit_assignments",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_offering_lecturers_school_id",
                schema: "curriculum",
                table: "offering_lecturers",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_profiles_school_id",
                schema: "curriculum",
                table: "lecturer_profiles",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_profiles_school_id_LecturerCode",
                schema: "curriculum",
                table: "lecturer_profiles",
                columns: new[] { "school_id", "LecturerCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_school_id",
                schema: "curriculum",
                table: "import_batches",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_home_classes_school_id",
                schema: "curriculum",
                table: "home_classes",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_grading_policies_school_id",
                schema: "curriculum",
                table: "grading_policies",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_scales_school_id",
                schema: "curriculum",
                table: "grade_scales",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_changes_school_id",
                schema: "curriculum",
                table: "grade_changes",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_courses_school_id",
                schema: "curriculum",
                table: "courses",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_plos_school_id",
                schema: "curriculum",
                table: "course_plos",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_offerings_school_id",
                schema: "curriculum",
                table: "course_offerings",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_school_id",
                schema: "curriculum",
                table: "course_enrollments",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_clos_school_id",
                schema: "curriculum",
                table: "clos",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_clo_plos_school_id",
                schema: "curriculum",
                table: "clo_plos",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_school_id",
                schema: "curriculum",
                table: "audit_events",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_components_school_id",
                schema: "curriculum",
                table: "assessment_components",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_component_clos_school_id",
                schema: "curriculum",
                table: "assessment_component_clos",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_academic_students_school_id",
                schema: "curriculum",
                table: "academic_students",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_correction_requests_EnrollmentId",
                schema: "curriculum",
                table: "grade_correction_requests",
                column: "EnrollmentId",
                unique: true,
                filter: "\"Status\" = 'pending'");

            migrationBuilder.CreateIndex(
                name: "IX_grade_correction_requests_OfferingId",
                schema: "curriculum",
                table: "grade_correction_requests",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_grade_correction_requests_school_id",
                schema: "curriculum",
                table: "grade_correction_requests",
                column: "school_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grade_correction_requests",
                schema: "curriculum");

            migrationBuilder.DropIndex(
                name: "IX_student_grades_school_id",
                schema: "curriculum",
                table: "student_grades");

            migrationBuilder.DropIndex(
                name: "IX_source_documents_school_id",
                schema: "curriculum",
                table: "source_documents");

            migrationBuilder.DropIndex(
                name: "IX_source_course_rows_school_id",
                schema: "curriculum",
                table: "source_course_rows");

            migrationBuilder.DropIndex(
                name: "IX_programs_school_id",
                schema: "curriculum",
                table: "programs");

            migrationBuilder.DropIndex(
                name: "IX_program_versions_school_id",
                schema: "curriculum",
                table: "program_versions");

            migrationBuilder.DropIndex(
                name: "IX_program_courses_school_id",
                schema: "curriculum",
                table: "program_courses");

            migrationBuilder.DropIndex(
                name: "IX_plos_school_id",
                schema: "curriculum",
                table: "plos");

            migrationBuilder.DropIndex(
                name: "IX_outcome_snapshots_school_id",
                schema: "curriculum",
                table: "outcome_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_outcome_policies_school_id",
                schema: "curriculum",
                table: "outcome_policies");

            migrationBuilder.DropIndex(
                name: "IX_org_units_school_id",
                schema: "curriculum",
                table: "org_units");

            migrationBuilder.DropIndex(
                name: "IX_org_unit_assignments_school_id",
                schema: "curriculum",
                table: "org_unit_assignments");

            migrationBuilder.DropIndex(
                name: "IX_offering_lecturers_school_id",
                schema: "curriculum",
                table: "offering_lecturers");

            migrationBuilder.DropIndex(
                name: "IX_lecturer_profiles_school_id",
                schema: "curriculum",
                table: "lecturer_profiles");

            migrationBuilder.DropIndex(
                name: "IX_lecturer_profiles_school_id_LecturerCode",
                schema: "curriculum",
                table: "lecturer_profiles");

            migrationBuilder.DropIndex(
                name: "IX_import_batches_school_id",
                schema: "curriculum",
                table: "import_batches");

            migrationBuilder.DropIndex(
                name: "IX_home_classes_school_id",
                schema: "curriculum",
                table: "home_classes");

            migrationBuilder.DropIndex(
                name: "IX_grading_policies_school_id",
                schema: "curriculum",
                table: "grading_policies");

            migrationBuilder.DropIndex(
                name: "IX_grade_scales_school_id",
                schema: "curriculum",
                table: "grade_scales");

            migrationBuilder.DropIndex(
                name: "IX_grade_changes_school_id",
                schema: "curriculum",
                table: "grade_changes");

            migrationBuilder.DropIndex(
                name: "IX_courses_school_id",
                schema: "curriculum",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_course_plos_school_id",
                schema: "curriculum",
                table: "course_plos");

            migrationBuilder.DropIndex(
                name: "IX_course_offerings_school_id",
                schema: "curriculum",
                table: "course_offerings");

            migrationBuilder.DropIndex(
                name: "IX_course_enrollments_school_id",
                schema: "curriculum",
                table: "course_enrollments");

            migrationBuilder.DropIndex(
                name: "IX_clos_school_id",
                schema: "curriculum",
                table: "clos");

            migrationBuilder.DropIndex(
                name: "IX_clo_plos_school_id",
                schema: "curriculum",
                table: "clo_plos");

            migrationBuilder.DropIndex(
                name: "IX_audit_events_school_id",
                schema: "curriculum",
                table: "audit_events");

            migrationBuilder.DropIndex(
                name: "IX_assessment_components_school_id",
                schema: "curriculum",
                table: "assessment_components");

            migrationBuilder.DropIndex(
                name: "IX_assessment_component_clos_school_id",
                schema: "curriculum",
                table: "assessment_component_clos");

            migrationBuilder.DropIndex(
                name: "IX_academic_students_school_id",
                schema: "curriculum",
                table: "academic_students");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "student_grades");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "source_documents");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "source_course_rows");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "programs");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "program_versions");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "program_courses");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "plos");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "outcome_snapshots");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "outcome_policies");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "org_units");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "org_unit_assignments");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "offering_lecturers");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "lecturer_profiles");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "import_batches");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "home_classes");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "grading_policies");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "grade_scales");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "grade_changes");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "course_plos");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "course_offerings");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "course_enrollments");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "clos");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "clo_plos");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "audit_events");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "assessment_components");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "assessment_component_clos");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "curriculum",
                table: "academic_students");

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_profiles_LecturerCode",
                schema: "curriculum",
                table: "lecturer_profiles",
                column: "LecturerCode",
                unique: true);
        }
    }
}
