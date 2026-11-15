using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Curriculum.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutcomeAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_events",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Entity = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outcome_policies",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VersionId = table.Column<long>(type: "bigint", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    Json = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outcome_policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outcome_policies_program_versions_VersionId",
                        column: x => x.VersionId,
                        principalSchema: "curriculum",
                        principalTable: "program_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outcome_snapshots",
                schema: "curriculum",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<long>(type: "bigint", nullable: false),
                    Json = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outcome_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outcome_snapshots_program_versions_VersionId",
                        column: x => x.VersionId,
                        principalSchema: "curriculum",
                        principalTable: "program_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_CreatedAt",
                schema: "curriculum",
                table: "audit_events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_outcome_policies_VersionId_Revision",
                schema: "curriculum",
                table: "outcome_policies",
                columns: new[] { "VersionId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outcome_snapshots_VersionId_CreatedAt",
                schema: "curriculum",
                table: "outcome_snapshots",
                columns: new[] { "VersionId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_events",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "outcome_policies",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "outcome_snapshots",
                schema: "curriculum");
        }
    }
}
