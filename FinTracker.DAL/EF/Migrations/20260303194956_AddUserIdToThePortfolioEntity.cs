using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTracker.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToThePortfolioEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PortfolioEntities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PortfolioEntities");
        }
    }
}
