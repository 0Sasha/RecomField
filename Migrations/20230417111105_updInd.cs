using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class updInd : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniqueIndex",
                table: "Review");
        }
    }
}
