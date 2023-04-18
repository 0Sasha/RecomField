using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUnInProd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UniqueIndex",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "Product");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Id",
                table: "Product",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Product_Id",
                table: "Product");

            migrationBuilder.AddColumn<int>(
                name: "UniqueIndex",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "UniqueIndex",
                table: "Product",
                column: "UniqueIndex",
                unique: true);
        }
    }
}
