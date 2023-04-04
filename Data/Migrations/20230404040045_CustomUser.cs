using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Data.Migrations
{
    /// <inheritdoc />
    public partial class CustomUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SettingsId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinuteTimeOffset = table.Column<int>(type: "int", nullable: false),
                    DarkTheme = table.Column<bool>(type: "bit", nullable: false),
                    InterfaceLanguage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SettingsId",
                table: "AspNetUsers",
                column: "SettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserSettings_SettingsId",
                table: "AspNetUsers",
                column: "SettingsId",
                principalTable: "UserSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserSettings_SettingsId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SettingsId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SettingsId",
                table: "AspNetUsers");
        }
    }
}
