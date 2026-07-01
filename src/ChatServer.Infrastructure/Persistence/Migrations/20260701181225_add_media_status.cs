using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_media_status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "media",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "media");
        }
    }
}
