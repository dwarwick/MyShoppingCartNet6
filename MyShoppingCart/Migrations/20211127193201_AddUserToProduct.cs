using Microsoft.EntityFrameworkCore.Migrations;

namespace MyShoppingCart.Migrations
{
    public partial class AddUserToProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "applicationUserId",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_applicationUserId",
                table: "Products",
                column: "applicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_applicationUserId",
                table: "Products",
                column: "applicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_applicationUserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_applicationUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "applicationUserId",
                table: "Products");
        }
    }
}
