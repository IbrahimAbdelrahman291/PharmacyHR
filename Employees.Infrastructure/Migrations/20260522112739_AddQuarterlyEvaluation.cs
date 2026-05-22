using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employees.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuarterlyEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuarterlyEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EvaluatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quarter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuarterlyEvaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuarterlyEvaluationId = table.Column<int>(type: "int", nullable: false),
                    EvaluationCriteriaId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationResults_QuarterlyEvaluations_QuarterlyEvaluationId",
                        column: x => x.QuarterlyEvaluationId,
                        principalTable: "QuarterlyEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationResults_QuarterlyEvaluationId",
                table: "EvaluationResults",
                column: "QuarterlyEvaluationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationResults");

            migrationBuilder.DropTable(
                name: "QuarterlyEvaluations");
        }
    }
}
