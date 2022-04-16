using Microsoft.EntityFrameworkCore.Migrations;

namespace MyShoppingCart.Migrations
{
    public partial class AddSellerAccountRegisterVM : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SellerAccount",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellerAccount",
                table: "AspNetUsers");
        }
    }
}
