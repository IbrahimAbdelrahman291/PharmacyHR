using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Requests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addNotificationForEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByEmployee",
                table: "ResignationRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByEmployee",
                table: "OvertimeRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByEmployee",
                table: "HolidayRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByEmployee",
                table: "ForgetedHoursRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByEmployee",
                table: "ComplaintRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByEmployee",
                table: "BorrowRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeenByEmployee",
                table: "ResignationRequests");

            migrationBuilder.DropColumn(
                name: "IsSeenByEmployee",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "IsSeenByEmployee",
                table: "HolidayRequests");

            migrationBuilder.DropColumn(
                name: "IsSeenByEmployee",
                table: "ForgetedHoursRequests");

            migrationBuilder.DropColumn(
                name: "IsSeenByEmployee",
                table: "ComplaintRequests");

            migrationBuilder.DropColumn(
                name: "IsSeenByEmployee",
                table: "BorrowRequests");
        }
    }
}
