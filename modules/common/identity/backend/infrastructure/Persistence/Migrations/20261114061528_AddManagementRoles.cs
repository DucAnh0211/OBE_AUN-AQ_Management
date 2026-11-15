using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Identity.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManagementRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "security_users_role_ck",
                schema: "security",
                table: "users");

            migrationBuilder.AddCheckConstraint(
                name: "security_users_role_ck",
                schema: "security",
                table: "users",
                sql: "role IN ('admin', 'lecturer', 'student', 'faculty_manager', 'department_manager')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "security_users_role_ck",
                schema: "security",
                table: "users");

            migrationBuilder.AddCheckConstraint(
                name: "security_users_role_ck",
                schema: "security",
                table: "users",
                sql: "role IN ('admin', 'lecturer', 'student')");
        }
    }
}
