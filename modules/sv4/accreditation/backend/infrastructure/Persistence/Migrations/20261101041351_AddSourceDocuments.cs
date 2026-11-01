using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObeAunQa.Modules.Accreditation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "source_documents",
                schema: "accreditation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Bucket = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisabledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisabledBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_documents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_source_documents_Bucket_ObjectKey",
                schema: "accreditation",
                table: "source_documents",
                columns: new[] { "Bucket", "ObjectKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_documents_CreatedAt",
                schema: "accreditation",
                table: "source_documents",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "source_documents",
                schema: "accreditation");
        }
    }
}
