using Microsoft.EntityFrameworkCore.Migrations;

namespace MyShoppingCart.Migrations
{
    public partial class DomainBoundToUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DomainBound",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DomainBound",
                table: "AspNetUsers");
        }
    }
}
