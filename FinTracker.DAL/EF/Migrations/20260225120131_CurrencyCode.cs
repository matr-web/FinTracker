using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTracker.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class CurrencyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "Holdings",
                newName: "CurrencyCode");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "Histories",
                newName: "CurrencyCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrencyCode",
                table: "Holdings",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "CurrencyCode",
                table: "Histories",
                newName: "Currency");
        }
    }
}
