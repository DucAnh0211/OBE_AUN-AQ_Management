using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Knowledge.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChatRoleScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerRole",
                schema: "knowledge",
                table: "chat_threads",
                type: "text",
                nullable: false,
                defaultValue: "");
            migrationBuilder.Sql("UPDATE knowledge.chat_threads t SET \"OwnerRole\"=u.role FROM security.users u WHERE t.\"OwnerId\"=u.id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerRole",
                schema: "knowledge",
                table: "chat_threads");
        }
    }
}
