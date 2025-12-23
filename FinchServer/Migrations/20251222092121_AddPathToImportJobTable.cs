using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinchServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPathToImportJobTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "import_jobs",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "path",
                table: "import_jobs");
        }
    }
}
