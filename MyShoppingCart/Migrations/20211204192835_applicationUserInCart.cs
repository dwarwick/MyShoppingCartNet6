using Microsoft.EntityFrameworkCore.Migrations;

namespace MyShoppingCart.Migrations
{
    public partial class applicationUserInCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellerAccount",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "applicationUserId",
                table: "ShoppingCartItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItems_applicationUserId",
                table: "ShoppingCartItems",
                column: "applicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCartItems_AspNetUsers_applicationUserId",
                table: "ShoppingCartItems",
                column: "applicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCartItems_AspNetUsers_applicationUserId",
                table: "ShoppingCartItems");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCartItems_applicationUserId",
                table: "ShoppingCartItems");

            migrationBuilder.DropColumn(
                name: "applicationUserId",
                table: "ShoppingCartItems");

            migrationBuilder.AddColumn<bool>(
                name: "SellerAccount",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
