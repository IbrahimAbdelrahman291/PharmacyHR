using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Requests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateComplaintRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByAreaManager",
                table: "ComplaintRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByCEO",
                table: "ComplaintRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeenByAreaManager",
                table: "ComplaintRequests");

            migrationBuilder.DropColumn(
                name: "IsSeenByCEO",
                table: "ComplaintRequests");
        }
    }
}
