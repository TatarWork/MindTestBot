using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MindTestBot.Migrations
{
    /// <inheritdoc />
    public partial class CorrectDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestSessionEvents");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "UserTestStates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "UserTestStates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "UserTestStates",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "UserTestStates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserTestStates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "UserTestStates");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "UserTestStates");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "UserTestStates");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "UserTestStates");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserTestStates");

            migrationBuilder.CreateTable(
                name: "TestSessionEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EventData = table.Column<string>(type: "jsonb", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSessionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSessionEvents_UserTestStates_ChatId",
                        column: x => x.ChatId,
                        principalTable: "UserTestStates",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestSessionEvents_ChatId",
                table: "TestSessionEvents",
                column: "ChatId");
        }
    }
}
