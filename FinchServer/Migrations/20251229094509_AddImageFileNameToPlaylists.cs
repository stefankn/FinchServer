using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinchServer.Migrations
{
    /// <inheritdoc />
    public partial class AddImageFileNameToPlaylists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_file_name",
                table: "playlists",
                type: "TEXT",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_file_name",
                table: "playlists");
        }
    }
}
