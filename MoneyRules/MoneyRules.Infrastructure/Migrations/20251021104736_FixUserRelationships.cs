using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoneyRules.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Settings_UserId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SettingsId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SettingsId",
                table: "Settings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.AddColumn<int>(
                name: "SettingsId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SettingsId",
                table: "Settings",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                column: "SettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_UserId",
                table: "Settings",
                column: "UserId",
                unique: true);
        }
    }
}
