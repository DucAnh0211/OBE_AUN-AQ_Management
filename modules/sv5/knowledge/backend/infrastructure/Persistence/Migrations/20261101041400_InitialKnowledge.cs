using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Knowledge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialKnowledge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "knowledge");

            migrationBuilder.CreateTable(
                name: "ingestion_jobs",
                schema: "knowledge",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PipelineVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Warning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Lease = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    ChunkCount = table.Column<int>(type: "integer", nullable: false),
                    Dimensions = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingestion_jobs", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "chunks",
                schema: "knowledge",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordinal = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    Locator = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartOffset = table.Column<int>(type: "integer", nullable: false),
                    Embedding = table.Column<float[]>(type: "real[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chunks", x => new { x.DocumentId, x.Ordinal });
                    table.ForeignKey(
                        name: "FK_chunks_ingestion_jobs_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "knowledge",
                        principalTable: "ingestion_jobs",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_jobs_Status_UpdatedAt",
                schema: "knowledge",
                table: "ingestion_jobs",
                columns: new[] { "Status", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chunks",
                schema: "knowledge");

            migrationBuilder.DropTable(
                name: "ingestion_jobs",
                schema: "knowledge");
        }
    }
}
