using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTracker.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class DebtEntity_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfInstallmentsLeft",
                table: "Debts");

            migrationBuilder.CreateTable(
                name: "Installments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DebtId = table.Column<int>(type: "int", nullable: false),
                    AmountLeft = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumberOfInstallment = table.Column<int>(type: "int", nullable: true),
                    RepaymentDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Installments_Debts_DebtId",
                        column: x => x.DebtId,
                        principalTable: "Debts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Installments_DebtId",
                table: "Installments",
                column: "DebtId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Installments");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfInstallmentsLeft",
                table: "Debts",
                type: "int",
                nullable: true);
        }
    }
}
