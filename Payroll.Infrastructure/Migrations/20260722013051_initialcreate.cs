using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Borrows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlyEmployeeDataId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    DateOfBorrow = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyEmployeeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Hours = table.Column<double>(type: "float", nullable: true),
                    HoursOverTime = table.Column<double>(type: "float", nullable: true),
                    ForgetedHours = table.Column<double>(type: "float", nullable: true),
                    Target = table.Column<double>(type: "float", nullable: true),
                    Insurence = table.Column<double>(type: "float", nullable: true),
                    HolidayHours = table.Column<double>(type: "float", nullable: true),
                    SalaryPerHour = table.Column<double>(type: "float", nullable: true),
                    TotalSalary = table.Column<double>(type: "float", nullable: true),
                    TotalDiscounts = table.Column<double>(type: "float", nullable: true),
                    TotalContractDiscount = table.Column<double>(type: "float", nullable: true),
                    TotalBouns = table.Column<double>(type: "float", nullable: true),
                    TotalBorrows = table.Column<double>(type: "float", nullable: true),
                    totalInstallmentBorrow = table.Column<double>(type: "float", nullable: true),
                    TotalCashBorrows = table.Column<double>(type: "float", nullable: true),
                    Holidaies = table.Column<int>(type: "int", nullable: true),
                    NetSalary = table.Column<double>(type: "float", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyEmployeeData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bonuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlyEmployeeDataId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    DateOfBonus = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bonuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bonuses_MonthlyEmployeeData_MonthlyEmployeeDataId",
                        column: x => x.MonthlyEmployeeDataId,
                        principalTable: "MonthlyEmployeeData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashBorrows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlyEmployeeDataId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    DateOfBorrow = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBorrows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashBorrows_MonthlyEmployeeData_MonthlyEmployeeDataId",
                        column: x => x.MonthlyEmployeeDataId,
                        principalTable: "MonthlyEmployeeData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractDiscounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlyEmployeeDataId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    ReasonOfDiscount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractDiscounts_MonthlyEmployeeData_MonthlyEmployeeDataId",
                        column: x => x.MonthlyEmployeeDataId,
                        principalTable: "MonthlyEmployeeData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlyEmployeeDataId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    ReasonOfDiscount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discounts_MonthlyEmployeeData_MonthlyEmployeeDataId",
                        column: x => x.MonthlyEmployeeDataId,
                        principalTable: "MonthlyEmployeeData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_MonthlyEmployeeDataId",
                table: "Bonuses",
                column: "MonthlyEmployeeDataId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBorrows_MonthlyEmployeeDataId",
                table: "CashBorrows",
                column: "MonthlyEmployeeDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDiscounts_MonthlyEmployeeDataId",
                table: "ContractDiscounts",
                column: "MonthlyEmployeeDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_MonthlyEmployeeDataId",
                table: "Discounts",
                column: "MonthlyEmployeeDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bonuses");

            migrationBuilder.DropTable(
                name: "Borrows");

            migrationBuilder.DropTable(
                name: "CashBorrows");

            migrationBuilder.DropTable(
                name: "ContractDiscounts");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "MonthlyEmployeeData");
        }
    }
}
