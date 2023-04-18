using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class AddUnIndexesExceptProd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UniqueIndex",
                table: "ReviewTag",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UniqueIndex",
                table: "ReviewComment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "UniqueIndex",
                table: "ReviewTag",
                column: "UniqueIndex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UniqueIndex",
                table: "ReviewComment",
                column: "UniqueIndex",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UniqueIndex",
                table: "ReviewTag");

            migrationBuilder.DropIndex(
                name: "UniqueIndex",
                table: "ReviewComment");

            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "ReviewTag");

            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "ReviewComment");
        }
    }
}
