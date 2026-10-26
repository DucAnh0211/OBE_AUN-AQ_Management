using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ObeAunQa.Modules.Curriculum.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCurriculum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "curriculum");

            migrationBuilder.CreateTable(
                name: "courses",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    institutional_code = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("courses_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "programs",
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
                    table.PrimaryKey("programs_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "progression_codes",
                schema: "curriculum",
                columns: table => new
                {
                    code = table.Column<char>(type: "character(1)", maxLength: 1, nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("progression_codes_pkey", x => x.code);
                    table.CheckConstraint("progression_codes_rank_check", "rank > 0");
                });

            migrationBuilder.CreateTable(
                name: "source_documents",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_kind = table.Column<string>(type: "text", nullable: false),
                    sha256 = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    object_key = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("source_documents_pkey", x => x.id);
                    table.CheckConstraint("source_documents_file_kind_check", "file_kind IN ('xlsx','docx','json')");
                });

            migrationBuilder.CreateTable(
                name: "weight_codes",
                schema: "curriculum",
                columns: table => new
                {
                    code = table.Column<char>(type: "character(1)", maxLength: 1, nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("weight_codes_pkey", x => x.code);
                    table.CheckConstraint("weight_codes_score_check", "score > 0");
                });

            migrationBuilder.CreateTable(
                name: "program_versions",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    program_id = table.Column<long>(type: "bigint", nullable: false),
                    version_code = table.Column<string>(type: "text", nullable: false),
                    dataset_status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'finalized'::text"),
                    source_version_id = table.Column<long>(type: "bigint", nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("program_versions_pkey", x => x.id);
                    table.CheckConstraint("program_versions_dataset_status_ck", "dataset_status IN ('draft','finalized','archived')");
                    table.ForeignKey(
                        name: "program_versions_program_id_fkey",
                        column: x => x.program_id,
                        principalSchema: "curriculum",
                        principalTable: "programs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "program_versions_source_version_id_fkey",
                        column: x => x.source_version_id,
                        principalSchema: "curriculum",
                        principalTable: "program_versions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "import_batches",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    program_version_id = table.Column<long>(type: "bigint", nullable: false),
                    dataset_document_id = table.Column<long>(type: "bigint", nullable: false),
                    workbook_document_id = table.Column<long>(type: "bigint", nullable: false),
                    syllabus_document_id = table.Column<long>(type: "bigint", nullable: false),
                    dataset_sha256 = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    imported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("import_batches_pkey", x => x.id);
                    table.ForeignKey(
                        name: "import_batches_dataset_document_id_fkey",
                        column: x => x.dataset_document_id,
                        principalSchema: "curriculum",
                        principalTable: "source_documents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "import_batches_program_version_id_fkey",
                        column: x => x.program_version_id,
                        principalSchema: "curriculum",
                        principalTable: "program_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "import_batches_syllabus_document_id_fkey",
                        column: x => x.syllabus_document_id,
                        principalSchema: "curriculum",
                        principalTable: "source_documents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "import_batches_workbook_document_id_fkey",
                        column: x => x.workbook_document_id,
                        principalSchema: "curriculum",
                        principalTable: "source_documents",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "plos",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    program_version_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    statement = table.Column<string>(type: "text", nullable: false),
                    level_code = table.Column<string>(type: "text", nullable: true),
                    source_cell = table.Column<string>(type: "text", nullable: true),
                    provenance = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'original_curriculum_map'::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("plos_pkey", x => x.id);
                    table.UniqueConstraint("plos_id_program_version_id_key", x => new { x.id, x.program_version_id });
                    table.ForeignKey(
                        name: "plos_program_version_id_fkey",
                        column: x => x.program_version_id,
                        principalSchema: "curriculum",
                        principalTable: "program_versions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "program_courses",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    program_version_id = table.Column<long>(type: "bigint", nullable: false),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    source_row = table.Column<int>(type: "integer", nullable: true),
                    course_code_source_value = table.Column<string>(type: "text", nullable: true),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    semester_source_value = table.Column<string>(type: "text", nullable: true),
                    dataset_status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'finalized'::text"),
                    quality_flags = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    course_code_snapshot = table.Column<string>(type: "text", nullable: true),
                    course_name_snapshot = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("program_courses_pkey", x => x.id);
                    table.UniqueConstraint("program_courses_id_program_version_id_key", x => new { x.id, x.program_version_id });
                    table.CheckConstraint("program_courses_credits_check", "credits > 0");
                    table.CheckConstraint("program_courses_dataset_status_ck", "dataset_status IN ('draft','finalized','archived')");
                    table.CheckConstraint("program_courses_display_order_ck", "display_order > 0");
                    table.ForeignKey(
                        name: "program_courses_course_id_fkey",
                        column: x => x.course_id,
                        principalSchema: "curriculum",
                        principalTable: "courses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "program_courses_program_version_id_fkey",
                        column: x => x.program_version_id,
                        principalSchema: "curriculum",
                        principalTable: "program_versions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "source_course_rows",
                schema: "curriculum",
                columns: table => new
                {
                    import_batch_id = table.Column<long>(type: "bigint", nullable: false),
                    source_row = table.Column<int>(type: "integer", nullable: false),
                    raw_data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("source_course_rows_pkey", x => new { x.import_batch_id, x.source_row });
                    table.ForeignKey(
                        name: "source_course_rows_import_batch_id_fkey",
                        column: x => x.import_batch_id,
                        principalSchema: "curriculum",
                        principalTable: "import_batches",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "clos",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    program_course_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    statement = table.Column<string>(type: "text", nullable: false),
                    level_code = table.Column<string>(type: "text", nullable: true),
                    provenance = table.Column<string>(type: "text", nullable: false),
                    source_reference = table.Column<string>(type: "jsonb", nullable: true),
                    assumption_note = table.Column<string>(type: "text", nullable: true),
                    dataset_status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'finalized'::text")
                },
                constraints: table =>
                {
                    table.PrimaryKey("clos_pkey", x => x.id);
                    table.UniqueConstraint("clos_id_program_course_id_key", x => new { x.id, x.program_course_id });
                    table.ForeignKey(
                        name: "clos_program_course_id_fkey",
                        column: x => x.program_course_id,
                        principalSchema: "curriculum",
                        principalTable: "program_courses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "course_plos",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    program_version_id = table.Column<long>(type: "bigint", nullable: false),
                    program_course_id = table.Column<long>(type: "bigint", nullable: false),
                    plo_id = table.Column<long>(type: "bigint", nullable: false),
                    weight_code = table.Column<char>(type: "character(1)", maxLength: 1, nullable: false),
                    progression_code = table.Column<char>(type: "character(1)", maxLength: 1, nullable: false),
                    provenance = table.Column<string>(type: "text", nullable: false),
                    fit = table.Column<string>(type: "text", nullable: true),
                    fit_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("course_plos_pkey", x => x.id);
                    table.UniqueConstraint("course_plos_id_program_course_id_key", x => new { x.id, x.program_course_id });
                    table.ForeignKey(
                        name: "course_plos_plo_id_program_version_id_fkey",
                        columns: x => new { x.plo_id, x.program_version_id },
                        principalSchema: "curriculum",
                        principalTable: "plos",
                        principalColumns: new[] { "id", "program_version_id" });
                    table.ForeignKey(
                        name: "course_plos_program_course_id_program_version_id_fkey",
                        columns: x => new { x.program_course_id, x.program_version_id },
                        principalSchema: "curriculum",
                        principalTable: "program_courses",
                        principalColumns: new[] { "id", "program_version_id" });
                    table.ForeignKey(
                        name: "course_plos_progression_code_fkey",
                        column: x => x.progression_code,
                        principalSchema: "curriculum",
                        principalTable: "progression_codes",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "course_plos_weight_code_fkey",
                        column: x => x.weight_code,
                        principalSchema: "curriculum",
                        principalTable: "weight_codes",
                        principalColumn: "code");
                });

            migrationBuilder.CreateTable(
                name: "clo_plos",
                schema: "curriculum",
                columns: table => new
                {
                    clo_id = table.Column<long>(type: "bigint", nullable: false),
                    course_plo_id = table.Column<long>(type: "bigint", nullable: false),
                    program_course_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("clo_plos_pkey", x => new { x.clo_id, x.course_plo_id });
                    table.ForeignKey(
                        name: "clo_plos_clo_id_program_course_id_fkey",
                        columns: x => new { x.clo_id, x.program_course_id },
                        principalSchema: "curriculum",
                        principalTable: "clos",
                        principalColumns: new[] { "id", "program_course_id" });
                    table.ForeignKey(
                        name: "clo_plos_course_plo_id_program_course_id_fkey",
                        columns: x => new { x.course_plo_id, x.program_course_id },
                        principalSchema: "curriculum",
                        principalTable: "course_plos",
                        principalColumns: new[] { "id", "program_course_id" });
                });

            migrationBuilder.InsertData(
                schema: "curriculum",
                table: "progression_codes",
                columns: new[] { "code", "rank" },
                values: new object[,]
                {
                    { 'E', 3 },
                    { 'I', 1 },
                    { 'R', 2 }
                });

            migrationBuilder.InsertData(
                schema: "curriculum",
                table: "weight_codes",
                columns: new[] { "code", "score" },
                values: new object[,]
                {
                    { 'X', 2 },
                    { 'Y', 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_clo_plos_clo_id_program_course_id",
                schema: "curriculum",
                table: "clo_plos",
                columns: new[] { "clo_id", "program_course_id" });

            migrationBuilder.CreateIndex(
                name: "IX_clo_plos_course_plo_id_program_course_id",
                schema: "curriculum",
                table: "clo_plos",
                columns: new[] { "course_plo_id", "program_course_id" });

            migrationBuilder.CreateIndex(
                name: "clos_program_course_id_code_key",
                schema: "curriculum",
                table: "clos",
                columns: new[] { "program_course_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "course_plos_program_course_id_plo_id_key",
                schema: "curriculum",
                table: "course_plos",
                columns: new[] { "program_course_id", "plo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_plos_plo_id_program_version_id",
                schema: "curriculum",
                table: "course_plos",
                columns: new[] { "plo_id", "program_version_id" });

            migrationBuilder.CreateIndex(
                name: "IX_course_plos_program_course_id_program_version_id",
                schema: "curriculum",
                table: "course_plos",
                columns: new[] { "program_course_id", "program_version_id" });

            migrationBuilder.CreateIndex(
                name: "IX_course_plos_progression_code",
                schema: "curriculum",
                table: "course_plos",
                column: "progression_code");

            migrationBuilder.CreateIndex(
                name: "IX_course_plos_weight_code",
                schema: "curriculum",
                table: "course_plos",
                column: "weight_code");

            migrationBuilder.CreateIndex(
                name: "courses_institutional_code_uq",
                schema: "curriculum",
                table: "courses",
                column: "institutional_code",
                unique: true,
                filter: "(institutional_code IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "import_batches_program_version_id_dataset_sha256_key",
                schema: "curriculum",
                table: "import_batches",
                columns: new[] { "program_version_id", "dataset_sha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_dataset_document_id",
                schema: "curriculum",
                table: "import_batches",
                column: "dataset_document_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_syllabus_document_id",
                schema: "curriculum",
                table: "import_batches",
                column: "syllabus_document_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_workbook_document_id",
                schema: "curriculum",
                table: "import_batches",
                column: "workbook_document_id");

            migrationBuilder.CreateIndex(
                name: "plos_program_version_id_code_key",
                schema: "curriculum",
                table: "plos",
                columns: new[] { "program_version_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_program_courses_course_id",
                schema: "curriculum",
                table: "program_courses",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "program_courses_active_course_uq",
                schema: "curriculum",
                table: "program_courses",
                columns: new[] { "program_version_id", "course_id" },
                unique: true,
                filter: "(archived_at IS NULL)");

            migrationBuilder.CreateIndex(
                name: "program_courses_program_version_id_source_row_key",
                schema: "curriculum",
                table: "program_courses",
                columns: new[] { "program_version_id", "source_row" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "program_courses_version_lookup_ix",
                schema: "curriculum",
                table: "program_courses",
                columns: new[] { "program_version_id", "display_order", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_program_versions_source_version_id",
                schema: "curriculum",
                table: "program_versions",
                column: "source_version_id");

            migrationBuilder.CreateIndex(
                name: "program_versions_current_uq",
                schema: "curriculum",
                table: "program_versions",
                column: "program_id",
                unique: true,
                filter: "(is_current AND (archived_at IS NULL))");

            migrationBuilder.CreateIndex(
                name: "program_versions_program_id_version_code_key",
                schema: "curriculum",
                table: "program_versions",
                columns: new[] { "program_id", "version_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "program_versions_program_lookup_ix",
                schema: "curriculum",
                table: "program_versions",
                columns: new[] { "program_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "programs_code_key",
                schema: "curriculum",
                table: "programs",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "source_documents_file_kind_sha256_key",
                schema: "curriculum",
                table: "source_documents",
                columns: new[] { "file_kind", "sha256" },
                unique: true);
            // Expression indexes and the legacy read-only view are PostgreSQL-specific.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX program_courses_active_code_uq
                ON curriculum.program_courses(program_version_id, UPPER(BTRIM(course_code_snapshot)))
                WHERE course_code_snapshot IS NOT NULL AND archived_at IS NULL;
                CREATE INDEX programs_active_lookup_ix ON curriculum.programs(UPPER(code), UPPER(name)) WHERE archived_at IS NULL;
                CREATE OR REPLACE VIEW curriculum.plo_balance AS
                WITH scores AS (
                    SELECT p.program_version_id, p.code AS plo_code,
                           COALESCE(SUM(pc.credits * w.score), 0)::numeric AS score
                    FROM curriculum.plos p
                    LEFT JOIN curriculum.course_plos cp ON cp.plo_id = p.id
                    LEFT JOIN curriculum.program_courses pc ON pc.id = cp.program_course_id
                    LEFT JOIN curriculum.weight_codes w ON w.code = cp.weight_code
                    GROUP BY p.program_version_id, p.code
                ), means AS (
                    SELECT scores.*, AVG(score) OVER (PARTITION BY program_version_id) AS mean_score
                    FROM scores
                )
                SELECT program_version_id, plo_code, score, mean_score,
                       ROUND((score - mean_score) / NULLIF(mean_score, 0), 4) AS deviation,
                       ABS((score - mean_score) / NULLIF(mean_score, 0)) > 0.20 AS exceeds_20_percent
                FROM means;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS curriculum.plo_balance;");
            migrationBuilder.DropTable(
                name: "clo_plos",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "source_course_rows",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "clos",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "course_plos",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "import_batches",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "plos",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "program_courses",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "progression_codes",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "weight_codes",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "source_documents",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "courses",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "program_versions",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "programs",
                schema: "curriculum");
        }
    }
}
