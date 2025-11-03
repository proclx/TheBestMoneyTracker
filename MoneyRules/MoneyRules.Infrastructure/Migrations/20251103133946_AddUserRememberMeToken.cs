using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyRules.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRememberMeToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyBudget",
                table: "Settings");

            migrationBuilder.AddColumn<string>(
                name: "RememberMeToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RememberMeTokenExpiry",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberMeToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RememberMeTokenExpiry",
                table: "Users");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyBudget",
                table: "Settings",
                type: "numeric",
                nullable: true);
        }
    }
}
