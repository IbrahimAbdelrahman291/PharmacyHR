using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Branches.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchTargetFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetNumberOfEmployees",
                table: "Branches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetSalaries",
                table: "Branches",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetNumberOfEmployees",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "TargetSalaries",
                table: "Branches");
        }
    }
}
