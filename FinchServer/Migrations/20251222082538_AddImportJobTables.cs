using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinchServer.Migrations
{
    /// <inheritdoc />
    public partial class AddImportJobTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "import_jobs",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "import_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    filename = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    path = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    error_message = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    import_job_id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_import_files_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalTable: "import_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_import_files_import_job_id",
                table: "import_files",
                column: "import_job_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "import_files");

            migrationBuilder.DropTable(
                name: "import_jobs");
        }
    }
}
