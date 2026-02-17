using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTracker.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddTickerSymbol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TickerSymbol",
                table: "Holdings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TickerSymbol",
                table: "Holdings");
        }
    }
}
