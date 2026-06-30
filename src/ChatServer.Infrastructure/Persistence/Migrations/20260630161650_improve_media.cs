using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class improve_media : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_media_client_sender_id",
                table: "media");

            migrationBuilder.DropPrimaryKey(
                name: "PK_media",
                table: "media");

            migrationBuilder.DropColumn(
                name: "id",
                table: "media");

            migrationBuilder.RenameColumn(
                name: "sender_id",
                table: "media",
                newName: "client_id");

            migrationBuilder.RenameIndex(
                name: "IX_media_sender_id",
                table: "media",
                newName: "IX_media_client_id");

            migrationBuilder.AddPrimaryKey(
                name: "media_id",
                table: "media",
                column: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_media_client_client_id",
                table: "media",
                column: "client_id",
                principalTable: "client",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_media_client_client_id",
                table: "media");

            migrationBuilder.DropPrimaryKey(
                name: "media_id",
                table: "media");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "media",
                newName: "sender_id");

            migrationBuilder.RenameIndex(
                name: "IX_media_client_id",
                table: "media",
                newName: "IX_media_sender_id");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "media",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_media",
                table: "media",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_media_client_sender_id",
                table: "media",
                column: "sender_id",
                principalTable: "client",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
