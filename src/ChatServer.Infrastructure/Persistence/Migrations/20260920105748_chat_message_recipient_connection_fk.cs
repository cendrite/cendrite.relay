using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class chat_message_recipient_connection_fk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chat_message_client_recipient_id",
                table: "chat_message");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_client_connection_id",
                table: "client",
                column: "connection_id");

            migrationBuilder.AddForeignKey(
                name: "FK_chat_message_client_recipient_id",
                table: "chat_message",
                column: "recipient_id",
                principalTable: "client",
                principalColumn: "connection_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chat_message_client_recipient_id",
                table: "chat_message");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_client_connection_id",
                table: "client");

            migrationBuilder.AddForeignKey(
                name: "FK_chat_message_client_recipient_id",
                table: "chat_message",
                column: "recipient_id",
                principalTable: "client",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
