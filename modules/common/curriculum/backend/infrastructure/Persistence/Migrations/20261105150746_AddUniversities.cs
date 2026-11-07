using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Curriculum.infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniversities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "universities",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("universities_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "universities_code_key",
                schema: "curriculum",
                table: "universities",
                column: "code",
                unique: true);

            migrationBuilder.AddColumn<long>(
                name: "university_id",
                schema: "curriculum",
                table: "programs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "university_id",
                schema: "curriculum",
                table: "courses",
                type: "bigint",
                nullable: true);

            // Existing data predates universities: keep every row under one placeholder the admin renames in the UI.
            // An empty database gets no placeholder, so the demo seeder and fresh installs start clean.
            migrationBuilder.Sql("""
                INSERT INTO curriculum.universities (code, name)
                SELECT 'TRUONG-01', 'Trường chưa đặt tên'
                WHERE EXISTS (SELECT 1 FROM curriculum.programs) OR EXISTS (SELECT 1 FROM curriculum.courses);

                UPDATE curriculum.programs SET university_id = (SELECT id FROM curriculum.universities WHERE code = 'TRUONG-01');
                UPDATE curriculum.courses SET university_id = (SELECT id FROM curriculum.universities WHERE code = 'TRUONG-01');
                """);

            migrationBuilder.AlterColumn<long>(
                name: "university_id",
                schema: "curriculum",
                table: "programs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "university_id",
                schema: "curriculum",
                table: "courses",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "programs_code_key",
                schema: "curriculum",
                table: "programs");

            migrationBuilder.DropIndex(
                name: "courses_institutional_code_uq",
                schema: "curriculum",
                table: "courses");

            migrationBuilder.CreateIndex(
                name: "programs_university_id_code_key",
                schema: "curriculum",
                table: "programs",
                columns: new[] { "university_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "courses_university_id_institutional_code_uq",
                schema: "curriculum",
                table: "courses",
                columns: new[] { "university_id", "institutional_code" },
                unique: true,
                filter: "(institutional_code IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "courses_university_id_fkey",
                schema: "curriculum",
                table: "courses",
                column: "university_id",
                principalSchema: "curriculum",
                principalTable: "universities",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "programs_university_id_fkey",
                schema: "curriculum",
                table: "programs",
                column: "university_id",
                principalSchema: "curriculum",
                principalTable: "universities",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "courses_university_id_fkey",
                schema: "curriculum",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "programs_university_id_fkey",
                schema: "curriculum",
                table: "programs");

            migrationBuilder.DropTable(
                name: "universities",
                schema: "curriculum");

            migrationBuilder.DropIndex(
                name: "programs_university_id_code_key",
                schema: "curriculum",
                table: "programs");

            migrationBuilder.DropIndex(
                name: "courses_university_id_institutional_code_uq",
                schema: "curriculum",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "university_id",
                schema: "curriculum",
                table: "programs");

            migrationBuilder.DropColumn(
                name: "university_id",
                schema: "curriculum",
                table: "courses");

            migrationBuilder.CreateIndex(
                name: "programs_code_key",
                schema: "curriculum",
                table: "programs",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "courses_institutional_code_uq",
                schema: "curriculum",
                table: "courses",
                column: "institutional_code",
                unique: true,
                filter: "(institutional_code IS NOT NULL)");
        }
    }
}
