using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUnIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UniqueIndex",
                table: "ReviewComment");

            migrationBuilder.DropIndex(
                name: "UniqueIndex",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "ReviewComment");

            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "Review");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComment_Id",
                table: "ReviewComment",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Review_Id",
                table: "Review",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReviewComment_Id",
                table: "ReviewComment");

            migrationBuilder.DropIndex(
                name: "IX_Review_Id",
                table: "Review");

            migrationBuilder.AddColumn<int>(
                name: "UniqueIndex",
                table: "ReviewComment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UniqueIndex",
                table: "Review",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "UniqueIndex",
                table: "ReviewComment",
                column: "UniqueIndex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UniqueIndex",
                table: "Review",
                column: "UniqueIndex",
                unique: true);
        }
    }
}
