using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ResumeHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdentity : Migration
    {
        // Data-preserving migration: the "AspNetUsers" table is RENAMED to "Users"
        // (rows and PasswordHash values are kept), unused Identity tables/columns are
        // dropped. Do NOT let EF drop+recreate this table or existing users are lost.
        private static readonly string[] ChildTables =
            ["Courses", "Education", "Experiences", "Languages", "Profiles", "Projects", "Skills"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Detach child FKs before renaming the principal table.
            foreach (var t in ChildTables)
                migrationBuilder.DropForeignKey(name: $"FK_{t}_AspNetUsers_UserId", table: t);

            // 2. Drop unused Identity satellite tables.
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserTokens");

            // 3. Rename the principal table, preserving all rows.
            migrationBuilder.RenameTable(name: "AspNetUsers", newName: "Users");
            migrationBuilder.Sql(
                @"ALTER TABLE ""Users"" RENAME CONSTRAINT ""PK_AspNetUsers"" TO ""PK_Users"";");

            // 4. Drop Identity-only indexes.
            migrationBuilder.DropIndex(name: "EmailIndex", table: "Users");
            migrationBuilder.DropIndex(name: "UserNameIndex", table: "Users");

            // 5. Drop Identity-only columns.
            foreach (var col in new[]
                {
                    "ConcurrencyStamp", "EmailConfirmed", "LockoutEnabled", "NormalizedUserName",
                    "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName"
                })
                migrationBuilder.DropColumn(name: col, table: "Users");

            // 6. Tighten column types/nullability to match the new model.
            migrationBuilder.AlterColumn<string>(
                name: "Email", table: "Users", type: "character varying(256)", maxLength: 256,
                nullable: false, defaultValue: "",
                oldClrType: typeof(string), oldType: "character varying(256)", oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail", table: "Users", type: "character varying(256)", maxLength: 256,
                nullable: false, defaultValue: "",
                oldClrType: typeof(string), oldType: "character varying(256)", oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash", table: "Users", type: "text", nullable: false, defaultValue: "",
                oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber", table: "Users", type: "character varying(40)", maxLength: 40,
                nullable: true, oldClrType: typeof(string), oldType: "text", oldNullable: true);

            foreach (var col in new[] { "FullName", "Headline", "Location" })
                migrationBuilder.AlterColumn<string>(
                    name: col, table: "Users", type: "character varying(200)", maxLength: 200,
                    nullable: true, oldClrType: typeof(string), oldType: "text", oldNullable: true);

            foreach (var col in new[] { "LinkedInUrl", "GitHubUrl", "WebsiteUrl" })
                migrationBuilder.AlterColumn<string>(
                    name: col, table: "Users", type: "character varying(300)", maxLength: 300,
                    nullable: true, oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash", table: "Users", type: "character varying(64)", maxLength: 64,
                nullable: true, oldClrType: typeof(string), oldType: "text", oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LockoutEnd", table: "Users", type: "timestamp with time zone", nullable: true,
                oldClrType: typeof(DateTimeOffset), oldType: "timestamp with time zone",
                oldNullable: true);

            // 7. Unique index for email lookups.
            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail", table: "Users", column: "NormalizedEmail",
                unique: true);

            // 8. Re-attach child FKs to the renamed table.
            foreach (var t in ChildTables)
                migrationBuilder.AddForeignKey(
                    name: $"FK_{t}_Users_UserId", table: t, column: "UserId",
                    principalTable: "Users", principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var t in ChildTables)
                migrationBuilder.DropForeignKey(name: $"FK_{t}_Users_UserId", table: t);

            migrationBuilder.DropIndex(name: "IX_Users_NormalizedEmail", table: "Users");

            // Revert column types/nullability.
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LockoutEnd", table: "Users", type: "timestamp with time zone", nullable: true,
                oldClrType: typeof(DateTime), oldType: "timestamp with time zone", oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash", table: "Users", type: "text", nullable: true,
                oldClrType: typeof(string), oldType: "character varying(64)", oldMaxLength: 64,
                oldNullable: true);

            foreach (var col in new[] { "LinkedInUrl", "GitHubUrl", "WebsiteUrl" })
                migrationBuilder.AlterColumn<string>(
                    name: col, table: "Users", type: "text", nullable: true,
                    oldClrType: typeof(string), oldType: "character varying(300)", oldMaxLength: 300,
                    oldNullable: true);

            foreach (var col in new[] { "FullName", "Headline", "Location" })
                migrationBuilder.AlterColumn<string>(
                    name: col, table: "Users", type: "text", nullable: true,
                    oldClrType: typeof(string), oldType: "character varying(200)", oldMaxLength: 200,
                    oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber", table: "Users", type: "text", nullable: true,
                oldClrType: typeof(string), oldType: "character varying(40)", oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash", table: "Users", type: "text", nullable: true,
                oldClrType: typeof(string), oldType: "text", oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail", table: "Users", type: "character varying(256)", maxLength: 256,
                nullable: true, oldClrType: typeof(string), oldType: "character varying(256)",
                oldMaxLength: 256, oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Email", table: "Users", type: "character varying(256)", maxLength: 256,
                nullable: true, oldClrType: typeof(string), oldType: "character varying(256)",
                oldMaxLength: 256, oldNullable: false);

            // Restore Identity-only columns.
            migrationBuilder.AddColumn<string>(name: "ConcurrencyStamp", table: "Users", type: "text", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "EmailConfirmed", table: "Users", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "LockoutEnabled", table: "Users", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "NormalizedUserName", table: "Users", type: "character varying(256)", maxLength: 256, nullable: true);
            migrationBuilder.AddColumn<bool>(name: "PhoneNumberConfirmed", table: "Users", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "SecurityStamp", table: "Users", type: "text", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "TwoFactorEnabled", table: "Users", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "UserName", table: "Users", type: "character varying(256)", maxLength: 256, nullable: true);

            migrationBuilder.Sql(
                @"ALTER TABLE ""Users"" RENAME CONSTRAINT ""PK_Users"" TO ""PK_AspNetUsers"";");
            migrationBuilder.RenameTable(name: "Users", newName: "AspNetUsers");

            migrationBuilder.CreateIndex(name: "EmailIndex", table: "AspNetUsers", column: "NormalizedEmail");
            migrationBuilder.CreateIndex(name: "UserNameIndex", table: "AspNetUsers", column: "NormalizedUserName", unique: true);

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId, principalTable: "AspNetUsers",
                        principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId, principalTable: "AspNetUsers",
                        principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId, principalTable: "AspNetUsers",
                        principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_AspNetUserClaims_UserId", table: "AspNetUserClaims", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_AspNetUserLogins_UserId", table: "AspNetUserLogins", column: "UserId");

            foreach (var t in ChildTables)
                migrationBuilder.AddForeignKey(
                    name: $"FK_{t}_AspNetUsers_UserId", table: t, column: "UserId",
                    principalTable: "AspNetUsers", principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
        }
    }
}
