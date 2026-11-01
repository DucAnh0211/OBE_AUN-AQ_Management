using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Knowledge.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatThreadsAndCheckpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chat_threads",
                schema: "knowledge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_threads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chat_exchanges",
                schema: "knowledge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AnswerJson = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Phase = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Lease = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    CheckpointId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_exchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chat_exchanges_chat_threads_ThreadId",
                        column: x => x.ThreadId,
                        principalSchema: "knowledge",
                        principalTable: "chat_threads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_checkpoints",
                schema: "knowledge",
                columns: table => new
                {
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckpointId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_checkpoints", x => new { x.ExchangeId, x.CheckpointId });
                    table.ForeignKey(
                        name: "FK_chat_checkpoints_chat_exchanges_ExchangeId",
                        column: x => x.ExchangeId,
                        principalSchema: "knowledge",
                        principalTable: "chat_exchanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_checkpoint_writes",
                schema: "knowledge",
                columns: table => new
                {
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckpointId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaskId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_checkpoint_writes", x => new { x.ExchangeId, x.CheckpointId, x.TaskId, x.Index });
                    table.ForeignKey(
                        name: "FK_chat_checkpoint_writes_chat_checkpoints_ExchangeId_Checkpoi~",
                        columns: x => new { x.ExchangeId, x.CheckpointId },
                        principalSchema: "knowledge",
                        principalTable: "chat_checkpoints",
                        principalColumns: new[] { "ExchangeId", "CheckpointId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chat_exchanges_Status_CreatedAt",
                schema: "knowledge",
                table: "chat_exchanges",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_chat_exchanges_ThreadId",
                schema: "knowledge",
                table: "chat_exchanges",
                column: "ThreadId",
                unique: true,
                filter: "\"Status\" IN ('queued','processing')");

            migrationBuilder.CreateIndex(
                name: "IX_chat_exchanges_ThreadId_CreatedAt",
                schema: "knowledge",
                table: "chat_exchanges",
                columns: new[] { "ThreadId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_chat_threads_OwnerId_UpdatedAt",
                schema: "knowledge",
                table: "chat_threads",
                columns: new[] { "OwnerId", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chat_checkpoint_writes",
                schema: "knowledge");

            migrationBuilder.DropTable(
                name: "chat_checkpoints",
                schema: "knowledge");

            migrationBuilder.DropTable(
                name: "chat_exchanges",
                schema: "knowledge");

            migrationBuilder.DropTable(
                name: "chat_threads",
                schema: "knowledge");
        }
    }
}
