using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Branches.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchTargetFeatureV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TargetHours",
                table: "Branches",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetHours",
                table: "Branches");
        }
    }
}
