using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Knowledge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOcrPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ForceOcr",
                schema: "knowledge",
                table: "ingestion_jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForceOcr",
                schema: "knowledge",
                table: "ingestion_jobs");
        }
    }
}
