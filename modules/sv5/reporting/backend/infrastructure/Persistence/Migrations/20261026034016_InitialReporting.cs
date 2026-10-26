using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Reporting.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reporting");

            migrationBuilder.CreateTable(
                name: "reports",
                schema: "reporting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_id = table.Column<long>(type: "bigint", nullable: false),
                    version_id = table.Column<long>(type: "bigint", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    snapshot_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    document = table.Column<string>(type: "jsonb", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("reports_pkey", x => x.id);
                    table.CheckConstraint("reports_kind_check", "kind IN ('course','clo','plo')");
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                schema: "reporting",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_events_pkey", x => x.id);
                    table.CheckConstraint("audit_events_action_check", "action IN ('created','viewed','download-docx','download-pdf','revoked')");
                    table.ForeignKey(
                        name: "audit_events_report_id_fkey",
                        column: x => x.report_id,
                        principalSchema: "reporting",
                        principalTable: "reports",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_report_id",
                schema: "reporting",
                table: "audit_events",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "reports_program_created_idx",
                schema: "reporting",
                table: "reports",
                columns: new[] { "program_id", "created_at", "id" },
                descending: new[] { false, true, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_events",
                schema: "reporting");

            migrationBuilder.DropTable(
                name: "reports",
                schema: "reporting");
        }
    }
}
