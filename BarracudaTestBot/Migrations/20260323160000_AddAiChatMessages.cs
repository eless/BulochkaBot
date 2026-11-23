using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarracudaTestBot.Migrations
{
    /// <inheritdoc />
    public partial class AddAiChatMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiChatMessages",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    ReplyToMessageId = table.Column<int>(type: "integer", nullable: true),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    AuthorName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatMessages", x => new { x.ChatId, x.MessageId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiChatMessages_CreatedAt",
                table: "AiChatMessages",
                column: "CreatedAt");

            migrationBuilder.Sql("""ALTER TABLE "AiChatMessages" ENABLE ROW LEVEL SECURITY;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiChatMessages");
        }
    }
}
