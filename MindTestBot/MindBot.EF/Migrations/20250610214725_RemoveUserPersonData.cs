using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindBot.EF.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserPersonData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "UserStates");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "UserStates");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "UserStates");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserStates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "UserStates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "UserStates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "UserStates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserStates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
