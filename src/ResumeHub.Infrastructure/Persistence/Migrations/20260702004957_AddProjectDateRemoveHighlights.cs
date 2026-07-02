using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDateRemoveHighlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Highlights",
                table: "Projects");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Projects",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "Highlights",
                table: "Projects",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }
    }
}
