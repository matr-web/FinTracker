using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTracker.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddQuanityAndBuyPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Holdings",
                newName: "BuyPrice");

            migrationBuilder.AddColumn<double>(
                name: "Quantity",
                table: "Holdings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Holdings");

            migrationBuilder.RenameColumn(
                name: "BuyPrice",
                table: "Holdings",
                newName: "Value");
        }
    }
}
