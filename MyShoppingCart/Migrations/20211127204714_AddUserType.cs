using Microsoft.EntityFrameworkCore.Migrations;

namespace MyShoppingCart.Migrations
{
    public partial class AddUserType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "AspNetUsers");
        }
    }
}
