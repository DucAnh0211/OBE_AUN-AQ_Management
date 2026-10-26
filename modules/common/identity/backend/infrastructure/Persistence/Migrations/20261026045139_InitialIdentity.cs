using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObeAunQa.Modules.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    email = table.Column<string>(type: "text", nullable: false),
                    normalized_email = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'active'::text"),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    token_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.CheckConstraint("security_users_failed_login_ck", "failed_login_count >= 0");
                    table.CheckConstraint("security_users_role_ck", "role IN ('admin', 'lecturer', 'student')");
                    table.CheckConstraint("security_users_status_ck", "status IN ('active', 'disabled')");
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    actor_user_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    target_type = table.Column<string>(type: "text", nullable: false),
                    target_id = table.Column<string>(type: "text", nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "audit_logs_actor_user_id_fkey",
                        column: x => x.actor_user_id,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lecturer_course_assignments",
                schema: "security",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    program_course_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_by = table.Column<long>(type: "bigint", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("lecturer_course_assignments_pkey", x => new { x.user_id, x.program_course_id });
                    table.ForeignKey(
                        name: "lecturer_course_assignments_assigned_by_fkey",
                        column: x => x.assigned_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "lecturer_course_assignments_program_course_id_fkey",
                        column: x => x.program_course_id,
                        principalSchema: "curriculum",
                        principalTable: "program_courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "lecturer_course_assignments_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_ip = table.Column<string>(type: "text", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "refresh_tokens_replaced_by_token_id_fkey",
                        column: x => x.replaced_by_token_id,
                        principalSchema: "security",
                        principalTable: "refresh_tokens",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "refresh_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_program_assignments",
                schema: "security",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    program_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_by = table.Column<long>(type: "bigint", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("student_program_assignments_pkey", x => new { x.user_id, x.program_id });
                    table.ForeignKey(
                        name: "student_program_assignments_assigned_by_fkey",
                        column: x => x.assigned_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "student_program_assignments_program_id_fkey",
                        column: x => x.program_id,
                        principalSchema: "curriculum",
                        principalTable: "programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "student_program_assignments_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "audit_logs_created_ix",
                schema: "security",
                table: "audit_logs",
                columns: new[] { "created_at", "id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_actor_user_id",
                schema: "security",
                table: "audit_logs",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_course_assignments_assigned_by",
                schema: "security",
                table: "lecturer_course_assignments",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "lecturer_assignments_course_ix",
                schema: "security",
                table: "lecturer_course_assignments",
                columns: new[] { "program_course_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_replaced_by_token_id",
                schema: "security",
                table: "refresh_tokens",
                column: "replaced_by_token_id");

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_token_hash_key",
                schema: "security",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_user_ix",
                schema: "security",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_student_program_assignments_assigned_by",
                schema: "security",
                table: "student_program_assignments",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "student_assignments_program_ix",
                schema: "security",
                table: "student_program_assignments",
                columns: new[] { "program_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "users_normalized_email_key",
                schema: "security",
                table: "users",
                column: "normalized_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "security");

            migrationBuilder.DropTable(
                name: "lecturer_course_assignments",
                schema: "security");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "security");

            migrationBuilder.DropTable(
                name: "student_program_assignments",
                schema: "security");

            migrationBuilder.DropTable(
                name: "users",
                schema: "security");
        }
    }
}
