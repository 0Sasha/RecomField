using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUnInTag2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UniqueIndex",
                table: "ReviewTag");

            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "ReviewTag");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTag_Id",
                table: "ReviewTag",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReviewTag_Id",
                table: "ReviewTag");

            migrationBuilder.AddColumn<int>(
                name: "UniqueIndex",
                table: "ReviewTag",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "UniqueIndex",
                table: "ReviewTag",
                column: "UniqueIndex",
                unique: true);
        }
    }
}
