using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserSettings_SettingsId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SettingsId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "SettingsId",
                table: "AspNetUsers",
                newName: "InterfaceLanguage");

            migrationBuilder.AddColumn<bool>(
                name: "DarkTheme",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DarkTheme",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "InterfaceLanguage",
                table: "AspNetUsers",
                newName: "SettingsId");

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
    }
}
