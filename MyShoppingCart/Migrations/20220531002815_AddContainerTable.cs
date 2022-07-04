using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShoppingCart.Migrations
{
    public partial class AddContainerTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "containers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LengthInch = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WidthInch = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HeightInch = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    shippingClassId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_containers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_containers_ShippingClasses_shippingClassId",
                        column: x => x.shippingClassId,
                        principalTable: "ShippingClasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_containers_shippingClassId",
                table: "containers",
                column: "shippingClassId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "containers");
        }
    }
}
