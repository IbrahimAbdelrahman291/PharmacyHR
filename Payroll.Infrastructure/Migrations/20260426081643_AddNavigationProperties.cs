using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Discounts_MonthlyEmployeeDataId",
                table: "Discounts",
                column: "MonthlyEmployeeDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDiscounts_MonthlyEmployeeDataId",
                table: "ContractDiscounts",
                column: "MonthlyEmployeeDataId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBorrows_MonthlyEmployeeDataId",
                table: "CashBorrows",
                column: "MonthlyEmployeeDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_MonthlyEmployeeDataId",
                table: "Bonuses",
                column: "MonthlyEmployeeDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bonuses_MonthlyEmployeeData_MonthlyEmployeeDataId",
                table: "Bonuses",
                column: "MonthlyEmployeeDataId",
                principalTable: "MonthlyEmployeeData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CashBorrows_MonthlyEmployeeData_MonthlyEmployeeDataId",
                table: "CashBorrows",
                column: "MonthlyEmployeeDataId",
                principalTable: "MonthlyEmployeeData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractDiscounts_MonthlyEmployeeData_MonthlyEmployeeDataId",
                table: "ContractDiscounts",
                column: "MonthlyEmployeeDataId",
                principalTable: "MonthlyEmployeeData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_MonthlyEmployeeData_MonthlyEmployeeDataId",
                table: "Discounts",
                column: "MonthlyEmployeeDataId",
                principalTable: "MonthlyEmployeeData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bonuses_MonthlyEmployeeData_MonthlyEmployeeDataId",
                table: "Bonuses");

            migrationBuilder.DropForeignKey(
                name: "FK_CashBorrows_MonthlyEmployeeData_MonthlyEmployeeDataId",
                table: "CashBorrows");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractDiscounts_MonthlyEmployeeData_MonthlyEmployeeDataId",
                table: "ContractDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_MonthlyEmployeeData_MonthlyEmployeeDataId",
                table: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Discounts_MonthlyEmployeeDataId",
                table: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_ContractDiscounts_MonthlyEmployeeDataId",
                table: "ContractDiscounts");

            migrationBuilder.DropIndex(
                name: "IX_CashBorrows_MonthlyEmployeeDataId",
                table: "CashBorrows");

            migrationBuilder.DropIndex(
                name: "IX_Bonuses_MonthlyEmployeeDataId",
                table: "Bonuses");
        }
    }
}
