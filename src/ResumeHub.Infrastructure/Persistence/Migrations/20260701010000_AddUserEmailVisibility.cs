using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEmailVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowEmailOnResume",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowEmailOnResume",
                table: "AspNetUsers");
        }
    }
}
