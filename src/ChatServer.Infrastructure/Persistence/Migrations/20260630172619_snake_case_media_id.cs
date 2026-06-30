using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class snake_case_media_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediaId",
                table: "media",
                newName: "media_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "media_id",
                table: "media",
                newName: "MediaId");
        }
    }
}
