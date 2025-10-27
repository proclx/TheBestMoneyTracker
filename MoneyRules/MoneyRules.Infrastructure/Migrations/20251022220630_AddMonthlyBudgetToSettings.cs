using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyRules.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyBudgetToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyBudget",
                table: "Settings",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyBudget",
                table: "Settings");
        }
    }
}
