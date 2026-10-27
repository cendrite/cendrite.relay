using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_client_user_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "client",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "idx_client_user_id",
                table: "client",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_client_user_id",
                table: "client");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "client");
        }
    }
}
