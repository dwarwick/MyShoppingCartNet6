using Microsoft.EntityFrameworkCore.Migrations;

namespace MyShoppingCart.Migrations
{
    public partial class AddedParentCategoryToCorrectTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentCategory",
                table: "ProductCategories");

            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryId",
                table: "ProductCategoryLookups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "ProductCategoryLookups");

            migrationBuilder.AddColumn<int>(
                name: "ParentCategory",
                table: "ProductCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
