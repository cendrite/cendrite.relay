using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cypherly.ChatServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class column_sender_Id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MessageToken",
                table: "Chatmessage",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageToken",
                table: "Chatmessage");
        }
    }
}
