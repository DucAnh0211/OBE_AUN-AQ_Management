using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Accreditation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAccreditation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "accreditation");

            migrationBuilder.CreateTable(
                name: "aun_frameworks",
                schema: "accreditation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    assessment_level = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'programme'::text"),
                    default_language = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'vi'::text"),
                    status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'draft'::text"),
                    source_title = table.Column<string>(type: "text", nullable: false),
                    source_url = table.Column<string>(type: "text", nullable: true),
                    content_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("aun_frameworks_pkey", x => x.id);
                    table.CheckConstraint("aun_frameworks_level_ck", "assessment_level = 'programme'");
                    table.CheckConstraint("aun_frameworks_status_ck", "status IN ('draft', 'published', 'retired')");
                });

            migrationBuilder.CreateTable(
                name: "aun_criteria",
                schema: "accreditation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    framework_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("aun_criteria_pkey", x => x.id);
                    table.CheckConstraint("aun_criteria_display_order_check", "display_order > 0");
                    table.ForeignKey(
                        name: "aun_criteria_framework_id_fkey",
                        column: x => x.framework_id,
                        principalSchema: "accreditation",
                        principalTable: "aun_frameworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aun_criterion_translations",
                schema: "accreditation",
                columns: table => new
                {
                    criterion_id = table.Column<long>(type: "bigint", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    translation_status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'draft'::text"),
                    translation_source = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("aun_criterion_translations_pkey", x => new { x.criterion_id, x.language_code });
                    table.CheckConstraint("aun_criterion_translations_translation_status_check", "translation_status IN ('official', 'reviewed', 'draft', 'machine_translated')");
                    table.ForeignKey(
                        name: "aun_criterion_translations_criterion_id_fkey",
                        column: x => x.criterion_id,
                        principalSchema: "accreditation",
                        principalTable: "aun_criteria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aun_requirements",
                schema: "accreditation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    criterion_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    source_page = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("aun_requirements_pkey", x => x.id);
                    table.CheckConstraint("aun_requirements_display_order_check", "display_order > 0");
                    table.CheckConstraint("aun_requirements_source_page_check", "source_page IS NULL OR source_page > 0");
                    table.ForeignKey(
                        name: "aun_requirements_criterion_id_fkey",
                        column: x => x.criterion_id,
                        principalSchema: "accreditation",
                        principalTable: "aun_criteria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aun_requirement_translations",
                schema: "accreditation",
                columns: table => new
                {
                    requirement_id = table.Column<long>(type: "bigint", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    statement = table.Column<string>(type: "text", nullable: false),
                    guidance = table.Column<string>(type: "text", nullable: true),
                    translation_status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'draft'::text"),
                    translation_source = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("aun_requirement_translations_pkey", x => new { x.requirement_id, x.language_code });
                    table.CheckConstraint("aun_requirement_translations_translation_status_check", "translation_status IN ('official', 'reviewed', 'draft', 'machine_translated')");
                    table.ForeignKey(
                        name: "aun_requirement_translations_requirement_id_fkey",
                        column: x => x.requirement_id,
                        principalSchema: "accreditation",
                        principalTable: "aun_requirements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "aun_criteria_framework_id_code_key",
                schema: "accreditation",
                table: "aun_criteria",
                columns: new[] { "framework_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "aun_criteria_framework_id_display_order_key",
                schema: "accreditation",
                table: "aun_criteria",
                columns: new[] { "framework_id", "display_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "aun_criteria_framework_ix",
                schema: "accreditation",
                table: "aun_criteria",
                columns: new[] { "framework_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "aun_frameworks_code_version_uq",
                schema: "accreditation",
                table: "aun_frameworks",
                columns: new[] { "code", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "aun_frameworks_status_ix",
                schema: "accreditation",
                table: "aun_frameworks",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "aun_requirements_criterion_id_code_key",
                schema: "accreditation",
                table: "aun_requirements",
                columns: new[] { "criterion_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "aun_requirements_criterion_id_display_order_key",
                schema: "accreditation",
                table: "aun_requirements",
                columns: new[] { "criterion_id", "display_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "aun_requirements_criterion_ix",
                schema: "accreditation",
                table: "aun_requirements",
                columns: new[] { "criterion_id", "display_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aun_criterion_translations",
                schema: "accreditation");

            migrationBuilder.DropTable(
                name: "aun_requirement_translations",
                schema: "accreditation");

            migrationBuilder.DropTable(
                name: "aun_requirements",
                schema: "accreditation");

            migrationBuilder.DropTable(
                name: "aun_criteria",
                schema: "accreditation");

            migrationBuilder.DropTable(
                name: "aun_frameworks",
                schema: "accreditation");
        }
    }
}
