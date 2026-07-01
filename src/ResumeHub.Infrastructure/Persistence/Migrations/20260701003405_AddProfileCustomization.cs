using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileCustomization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                table: "Profiles",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "#5b8cff");

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Profiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "dark");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccentColor",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Profiles");
        }
    }
}
