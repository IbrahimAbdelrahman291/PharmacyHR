using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Requests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOvertimeRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OvertimeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Hours = table.Column<double>(type: "float", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ControlApproval = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ControlUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ControlRejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaManagerApproval = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaManagerUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaManagerRejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HRApproval = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HRRejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSeenByHR = table.Column<bool>(type: "bit", nullable: false),
                    IsSeenByControl = table.Column<bool>(type: "bit", nullable: false),
                    IsSeenByAreaManager = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OvertimeRequests");
        }
    }
}
