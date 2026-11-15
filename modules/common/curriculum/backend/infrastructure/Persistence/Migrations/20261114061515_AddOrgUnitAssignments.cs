using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Curriculum.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgUnitAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "org_unit_assignments",
                schema: "curriculum",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    OrgUnitId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_unit_assignments", x => new { x.UserId, x.OrgUnitId });
                    table.ForeignKey(
                        name: "FK_org_unit_assignments_org_units_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalSchema: "curriculum",
                        principalTable: "org_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_org_unit_assignments_OrgUnitId",
                schema: "curriculum",
                table: "org_unit_assignments",
                column: "OrgUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "org_unit_assignments",
                schema: "curriculum");
        }
    }
}
