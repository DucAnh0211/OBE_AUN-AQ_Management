using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Knowledge.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "knowledge",
                table: "ingestion_jobs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "knowledge",
                table: "chunks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "knowledge",
                table: "chat_threads",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "knowledge",
                table: "chat_exchanges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "knowledge",
                table: "chat_checkpoints",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "knowledge",
                table: "chat_checkpoint_writes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_jobs_school_id",
                schema: "knowledge",
                table: "ingestion_jobs",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_chunks_school_id",
                schema: "knowledge",
                table: "chunks",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_threads_school_id",
                schema: "knowledge",
                table: "chat_threads",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_exchanges_school_id",
                schema: "knowledge",
                table: "chat_exchanges",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_checkpoints_school_id",
                schema: "knowledge",
                table: "chat_checkpoints",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_checkpoint_writes_school_id",
                schema: "knowledge",
                table: "chat_checkpoint_writes",
                column: "school_id");
            migrationBuilder.Sql(ObeAunQa.SharedKernel.SchoolTenancyUpgrade.Sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ingestion_jobs_school_id",
                schema: "knowledge",
                table: "ingestion_jobs");

            migrationBuilder.DropIndex(
                name: "IX_chunks_school_id",
                schema: "knowledge",
                table: "chunks");

            migrationBuilder.DropIndex(
                name: "IX_chat_threads_school_id",
                schema: "knowledge",
                table: "chat_threads");

            migrationBuilder.DropIndex(
                name: "IX_chat_exchanges_school_id",
                schema: "knowledge",
                table: "chat_exchanges");

            migrationBuilder.DropIndex(
                name: "IX_chat_checkpoints_school_id",
                schema: "knowledge",
                table: "chat_checkpoints");

            migrationBuilder.DropIndex(
                name: "IX_chat_checkpoint_writes_school_id",
                schema: "knowledge",
                table: "chat_checkpoint_writes");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "knowledge",
                table: "ingestion_jobs");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "knowledge",
                table: "chunks");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "knowledge",
                table: "chat_threads");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "knowledge",
                table: "chat_exchanges");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "knowledge",
                table: "chat_checkpoints");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "knowledge",
                table: "chat_checkpoint_writes");
        }
    }
}
