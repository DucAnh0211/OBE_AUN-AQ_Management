using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Accreditation.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramAccreditations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "program_accreditations",
                schema: "accreditation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    program_id = table.Column<long>(type: "bigint", nullable: false),
                    framework_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    assessed_on = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    verdict_rating = table.Column<int>(type: "integer", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("program_accreditations_pkey", x => x.id);
                    table.CheckConstraint("program_accreditations_rating_check", "verdict_rating IS NULL OR verdict_rating BETWEEN 1 AND 7");
                    table.CheckConstraint("program_accreditations_status_check", "status IN ('self_assessed', 'accredited', 'not_met')");
                    table.CheckConstraint("program_accreditations_valid_check", "valid_until IS NULL OR valid_until >= assessed_on");
                    table.ForeignKey(
                        name: "program_accreditations_framework_id_fkey",
                        column: x => x.framework_id,
                        principalSchema: "accreditation",
                        principalTable: "aun_frameworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_program_accreditations_framework_id",
                schema: "accreditation",
                table: "program_accreditations",
                column: "framework_id");

            migrationBuilder.CreateIndex(
                name: "program_accreditations_round_key",
                schema: "accreditation",
                table: "program_accreditations",
                columns: new[] { "program_id", "framework_id", "assessed_on" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "program_accreditations",
                schema: "accreditation");
        }
    }
}
