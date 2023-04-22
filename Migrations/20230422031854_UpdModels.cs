using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecomField.Migrations
{
    /// <inheritdoc />
    public partial class UpdModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductScore_AspNetUsers_SenderId",
                table: "ProductScore");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductScore_Product_EntityId",
                table: "ProductScore");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_AspNetUsers_AuthorId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComment_AspNetUsers_SenderId",
                table: "ReviewComment");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComment_Review_EntityId",
                table: "ReviewComment");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLike_AspNetUsers_SenderId",
                table: "ReviewLike");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLike_Review_EntityId",
                table: "ReviewLike");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewScore_AspNetUsers_SenderId",
                table: "ReviewScore");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewScore_Review_EntityId",
                table: "ReviewScore");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewTag_Review_EntityId",
                table: "ReviewTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewTag",
                table: "ReviewTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewScore",
                table: "ReviewScore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewLike",
                table: "ReviewLike");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewComment",
                table: "ReviewComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Review",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductScore",
                table: "ProductScore");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "ReviewTag",
                newName: "ReviewTags");

            migrationBuilder.RenameTable(
                name: "ReviewScore",
                newName: "ReviewScores");

            migrationBuilder.RenameTable(
                name: "ReviewLike",
                newName: "ReviewLikes");

            migrationBuilder.RenameTable(
                name: "ReviewComment",
                newName: "ReviewComments");

            migrationBuilder.RenameTable(
                name: "Review",
                newName: "Reviews");

            migrationBuilder.RenameTable(
                name: "ProductScore",
                newName: "ProductScores");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewTag_Id",
                table: "ReviewTags",
                newName: "IX_ReviewTags_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewTag_EntityId",
                table: "ReviewTags",
                newName: "IX_ReviewTags_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewScore_SenderId",
                table: "ReviewScores",
                newName: "IX_ReviewScores_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewScore_EntityId",
                table: "ReviewScores",
                newName: "IX_ReviewScores_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewLike_SenderId",
                table: "ReviewLikes",
                newName: "IX_ReviewLikes_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewLike_EntityId",
                table: "ReviewLikes",
                newName: "IX_ReviewLikes_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComment_SenderId",
                table: "ReviewComments",
                newName: "IX_ReviewComments_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComment_Id",
                table: "ReviewComments",
                newName: "IX_ReviewComments_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComment_EntityId",
                table: "ReviewComments",
                newName: "IX_ReviewComments_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Review_ProductId",
                table: "Reviews",
                newName: "IX_Reviews_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Review_Id",
                table: "Reviews",
                newName: "IX_Reviews_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Review_AuthorId",
                table: "Reviews",
                newName: "IX_Reviews_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductScore_SenderId",
                table: "ProductScores",
                newName: "IX_ProductScores_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductScore_EntityId",
                table: "ProductScores",
                newName: "IX_ProductScores_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_Id",
                table: "Products",
                newName: "IX_Products_Id");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Game_Trailer",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Series_Trailer",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewTags",
                table: "ReviewTags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewScores",
                table: "ReviewScores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewLikes",
                table: "ReviewLikes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewComments",
                table: "ReviewComments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reviews",
                table: "Reviews",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductScores",
                table: "ProductScores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductScores_AspNetUsers_SenderId",
                table: "ProductScores",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductScores_Products_EntityId",
                table: "ProductScores",
                column: "EntityId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComments_AspNetUsers_SenderId",
                table: "ReviewComments",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComments_Reviews_EntityId",
                table: "ReviewComments",
                column: "EntityId",
                principalTable: "Reviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLikes_AspNetUsers_SenderId",
                table: "ReviewLikes",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLikes_Reviews_EntityId",
                table: "ReviewLikes",
                column: "EntityId",
                principalTable: "Reviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_AuthorId",
                table: "Reviews",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewScores_AspNetUsers_SenderId",
                table: "ReviewScores",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewScores_Reviews_EntityId",
                table: "ReviewScores",
                column: "EntityId",
                principalTable: "Reviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewTags_Reviews_EntityId",
                table: "ReviewTags",
                column: "EntityId",
                principalTable: "Reviews",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductScores_AspNetUsers_SenderId",
                table: "ProductScores");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductScores_Products_EntityId",
                table: "ProductScores");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComments_AspNetUsers_SenderId",
                table: "ReviewComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComments_Reviews_EntityId",
                table: "ReviewComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLikes_AspNetUsers_SenderId",
                table: "ReviewLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLikes_Reviews_EntityId",
                table: "ReviewLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_AuthorId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewScores_AspNetUsers_SenderId",
                table: "ReviewScores");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewScores_Reviews_EntityId",
                table: "ReviewScores");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewTags_Reviews_EntityId",
                table: "ReviewTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewTags",
                table: "ReviewTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewScores",
                table: "ReviewScores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reviews",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewLikes",
                table: "ReviewLikes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewComments",
                table: "ReviewComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductScores",
                table: "ProductScores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Game_Trailer",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Series_Trailer",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "ReviewTags",
                newName: "ReviewTag");

            migrationBuilder.RenameTable(
                name: "ReviewScores",
                newName: "ReviewScore");

            migrationBuilder.RenameTable(
                name: "Reviews",
                newName: "Review");

            migrationBuilder.RenameTable(
                name: "ReviewLikes",
                newName: "ReviewLike");

            migrationBuilder.RenameTable(
                name: "ReviewComments",
                newName: "ReviewComment");

            migrationBuilder.RenameTable(
                name: "ProductScores",
                newName: "ProductScore");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewTags_Id",
                table: "ReviewTag",
                newName: "IX_ReviewTag_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewTags_EntityId",
                table: "ReviewTag",
                newName: "IX_ReviewTag_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewScores_SenderId",
                table: "ReviewScore",
                newName: "IX_ReviewScore_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewScores_EntityId",
                table: "ReviewScore",
                newName: "IX_ReviewScore_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ProductId",
                table: "Review",
                newName: "IX_Review_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_Id",
                table: "Review",
                newName: "IX_Review_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_AuthorId",
                table: "Review",
                newName: "IX_Review_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewLikes_SenderId",
                table: "ReviewLike",
                newName: "IX_ReviewLike_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewLikes_EntityId",
                table: "ReviewLike",
                newName: "IX_ReviewLike_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComments_SenderId",
                table: "ReviewComment",
                newName: "IX_ReviewComment_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComments_Id",
                table: "ReviewComment",
                newName: "IX_ReviewComment_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComments_EntityId",
                table: "ReviewComment",
                newName: "IX_ReviewComment_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductScores_SenderId",
                table: "ProductScore",
                newName: "IX_ProductScore_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductScores_EntityId",
                table: "ProductScore",
                newName: "IX_ProductScore_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Id",
                table: "Product",
                newName: "IX_Product_Id");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewTag",
                table: "ReviewTag",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewScore",
                table: "ReviewScore",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Review",
                table: "Review",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewLike",
                table: "ReviewLike",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewComment",
                table: "ReviewComment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductScore",
                table: "ProductScore",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductScore_AspNetUsers_SenderId",
                table: "ProductScore",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductScore_Product_EntityId",
                table: "ProductScore",
                column: "EntityId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_AspNetUsers_AuthorId",
                table: "Review",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Product_ProductId",
                table: "Review",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComment_AspNetUsers_SenderId",
                table: "ReviewComment",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComment_Review_EntityId",
                table: "ReviewComment",
                column: "EntityId",
                principalTable: "Review",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLike_AspNetUsers_SenderId",
                table: "ReviewLike",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLike_Review_EntityId",
                table: "ReviewLike",
                column: "EntityId",
                principalTable: "Review",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewScore_AspNetUsers_SenderId",
                table: "ReviewScore",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewScore_Review_EntityId",
                table: "ReviewScore",
                column: "EntityId",
                principalTable: "Review",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewTag_Review_EntityId",
                table: "ReviewTag",
                column: "EntityId",
                principalTable: "Review",
                principalColumn: "Id");
        }
    }
}
