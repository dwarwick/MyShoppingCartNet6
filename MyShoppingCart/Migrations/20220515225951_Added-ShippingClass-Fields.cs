using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShoppingCart.Migrations
{
    public partial class AddedShippingClassFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryTimeline",
                table: "ShippingClasses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCombinedLengthAndGirth",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxHeightInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxLengthInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxWeightOz",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxWidthInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinHeightInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinLengthInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinMachinableHeightInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinMachinableLengthInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinMachinableWidthInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinWidthInch",
                table: "ShippingClasses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "height",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "length",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "width",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryTimeline",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MaxCombinedLengthAndGirth",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MaxHeightInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MaxLengthInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MaxWeightOz",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MaxWidthInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MinHeightInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MinLengthInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MinMachinableHeightInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MinMachinableLengthInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MinMachinableWidthInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "MinWidthInch",
                table: "ShippingClasses");

            migrationBuilder.DropColumn(
                name: "height",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "length",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "width",
                table: "Products");
        }
    }
}
