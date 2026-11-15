using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Accreditation.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "aun_frameworks_code_version_uq",
                schema: "accreditation",
                table: "aun_frameworks");

            migrationBuilder.AddColumn<string>(
                name: "AccessScope",
                schema: "accreditation",
                table: "source_documents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "OwnerUserId",
                schema: "accreditation",
                table: "source_documents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "accreditation",
                table: "source_documents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "accreditation",
                table: "program_accreditations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "accreditation",
                table: "aun_requirements",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "accreditation",
                table: "aun_requirement_translations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "accreditation",
                table: "aun_frameworks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "accreditation",
                table: "aun_criterion_translations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "school_id",
                schema: "accreditation",
                table: "aun_criteria",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_documents_school_id",
                schema: "accreditation",
                table: "source_documents",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_program_accreditations_school_id",
                schema: "accreditation",
                table: "program_accreditations",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_aun_requirements_school_id",
                schema: "accreditation",
                table: "aun_requirements",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_aun_requirement_translations_school_id",
                schema: "accreditation",
                table: "aun_requirement_translations",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "aun_frameworks_code_version_uq",
                schema: "accreditation",
                table: "aun_frameworks",
                columns: new[] { "school_id", "code", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aun_frameworks_school_id",
                schema: "accreditation",
                table: "aun_frameworks",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_aun_criterion_translations_school_id",
                schema: "accreditation",
                table: "aun_criterion_translations",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_aun_criteria_school_id",
                schema: "accreditation",
                table: "aun_criteria",
                column: "school_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_source_documents_school_id",
                schema: "accreditation",
                table: "source_documents");

            migrationBuilder.DropIndex(
                name: "IX_program_accreditations_school_id",
                schema: "accreditation",
                table: "program_accreditations");

            migrationBuilder.DropIndex(
                name: "IX_aun_requirements_school_id",
                schema: "accreditation",
                table: "aun_requirements");

            migrationBuilder.DropIndex(
                name: "IX_aun_requirement_translations_school_id",
                schema: "accreditation",
                table: "aun_requirement_translations");

            migrationBuilder.DropIndex(
                name: "aun_frameworks_code_version_uq",
                schema: "accreditation",
                table: "aun_frameworks");

            migrationBuilder.DropIndex(
                name: "IX_aun_frameworks_school_id",
                schema: "accreditation",
                table: "aun_frameworks");

            migrationBuilder.DropIndex(
                name: "IX_aun_criterion_translations_school_id",
                schema: "accreditation",
                table: "aun_criterion_translations");

            migrationBuilder.DropIndex(
                name: "IX_aun_criteria_school_id",
                schema: "accreditation",
                table: "aun_criteria");

            migrationBuilder.DropColumn(
                name: "AccessScope",
                schema: "accreditation",
                table: "source_documents");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "accreditation",
                table: "source_documents");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "accreditation",
                table: "source_documents");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "accreditation",
                table: "program_accreditations");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "accreditation",
                table: "aun_requirements");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "accreditation",
                table: "aun_requirement_translations");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "accreditation",
                table: "aun_frameworks");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "accreditation",
                table: "aun_criterion_translations");

            migrationBuilder.DropColumn(
                name: "school_id",
                schema: "accreditation",
                table: "aun_criteria");

            migrationBuilder.CreateIndex(
                name: "aun_frameworks_code_version_uq",
                schema: "accreditation",
                table: "aun_frameworks",
                columns: new[] { "code", "version" },
                unique: true);
        }
    }
}
