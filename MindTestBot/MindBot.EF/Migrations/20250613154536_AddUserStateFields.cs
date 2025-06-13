using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindBot.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSendConsultNotifier",
                table: "UserStates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneForConsulting",
                table: "UserStates",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSendConsultNotifier",
                table: "UserStates");

            migrationBuilder.DropColumn(
                name: "PhoneForConsulting",
                table: "UserStates");
        }
    }
}
