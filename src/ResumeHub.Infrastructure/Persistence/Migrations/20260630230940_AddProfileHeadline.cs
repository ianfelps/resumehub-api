using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileHeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Headline",
                table: "Profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Headline",
                table: "Profiles");
        }
    }
}
