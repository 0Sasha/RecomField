using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class AddUnIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UniqueIndex",
                table: "Review",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "UniqueIndex",
                table: "Review",
                column: "UniqueIndex",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UniqueIndex",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "Review");
        }
    }
}
