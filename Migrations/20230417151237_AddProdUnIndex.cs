using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class AddProdUnIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UniqueIndex",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "Product");
        }
    }
}
