using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinuteTimeOffset",
                table: "UserSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinuteTimeOffset",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
