using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Identity.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "security_users_role_ck",
                schema: "security",
                table: "users");

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "security",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "security",
                table: "student_program_assignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "security",
                table: "lecturer_course_assignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "security",
                table: "audit_logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_school_id",
                schema: "security",
                table: "users",
                column: "school_id");

            migrationBuilder.Sql("UPDATE security.users SET role='school_admin', status='disabled' WHERE role IN ('faculty_manager','department_manager')");

            migrationBuilder.AddCheckConstraint(
                name: "security_users_role_ck",
                schema: "security",
                table: "users",
                sql: "role IN ('admin', 'lecturer', 'student', 'school_admin')");

            migrationBuilder.CreateIndex(
                name: "IX_student_program_assignments_school_id",
                schema: "security",
                table: "student_program_assignments",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_course_assignments_school_id",
                schema: "security",
                table: "lecturer_course_assignments",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_school_id",
                schema: "security",
                table: "audit_logs",
                column: "school_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_school_id",
                schema: "security",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "security_users_role_ck",
                schema: "security",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_student_program_assignments_school_id",
                schema: "security",
                table: "student_program_assignments");

            migrationBuilder.DropIndex(
                name: "IX_lecturer_course_assignments_school_id",
                schema: "security",
                table: "lecturer_course_assignments");

            migrationBuilder.DropIndex(
                name: "IX_audit_logs_school_id",
                schema: "security",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "security",
                table: "student_program_assignments");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "security",
                table: "lecturer_course_assignments");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "security",
                table: "audit_logs");

            migrationBuilder.AddCheckConstraint(
                name: "security_users_role_ck",
                schema: "security",
                table: "users",
                sql: "role IN ('admin', 'lecturer', 'student', 'faculty_manager', 'department_manager')");
        }
    }
}
