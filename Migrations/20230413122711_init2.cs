using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review");

            migrationBuilder.DropTable(
                name: "ReviewTag");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "ReviewerScore",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "UserScore",
                table: "Product");

            migrationBuilder.AddColumn<string>(
                name: "Cover",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Trailer",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductScore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductScore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductScore_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductScore_Product_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReviewScore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewScore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewScore_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewScore_Review_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Review",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_Review_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Review",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductScore_EntityId",
                table: "ProductScore",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductScore_SenderId",
                table: "ProductScore",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewScore_EntityId",
                table: "ReviewScore",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewScore_SenderId",
                table: "ReviewScore",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_ReviewId",
                table: "Tag",
                column: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review");

            migrationBuilder.DropTable(
                name: "ProductScore");

            migrationBuilder.DropTable(
                name: "ReviewScore");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropColumn(
                name: "Cover",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Trailer",
                table: "Product");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Review",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ReviewerScore",
                table: "Product",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UserScore",
                table: "Product",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "ReviewTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewTag_Review_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Review",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTag_EntityId",
                table: "ReviewTag",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
