using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Reporting.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "reporting",
                table: "reports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "reporting",
                table: "audit_events",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reports_school_id",
                schema: "reporting",
                table: "reports",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_school_id",
                schema: "reporting",
                table: "audit_events",
                column: "school_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reports_school_id",
                schema: "reporting",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_audit_events_school_id",
                schema: "reporting",
                table: "audit_events");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "reporting",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "reporting",
                table: "audit_events");
        }
    }
}
