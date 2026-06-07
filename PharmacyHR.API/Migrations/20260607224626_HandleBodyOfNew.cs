using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyHR.API.Migrations
{
    /// <inheritdoc />
    public partial class HandleBodyOfNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsArticles_Url",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "NewsArticles");

            migrationBuilder.AlterColumn<string>(
                name: "Uuid",
                table: "NewsArticles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Uuid",
                table: "NewsArticles",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsArticles_Uuid",
                table: "NewsArticles");

            migrationBuilder.AlterColumn<string>(
                name: "Uuid",
                table: "NewsArticles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "NewsArticles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "NewsArticles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Url",
                table: "NewsArticles",
                column: "Url",
                unique: true);
        }
    }
}
