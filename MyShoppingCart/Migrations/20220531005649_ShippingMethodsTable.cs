using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShoppingCart.Migrations
{
    public partial class ShippingMethodsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShippingMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    containerId = table.Column<int>(type: "int", nullable: true),
                    applicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingMethods_AspNetUsers_applicationUserId",
                        column: x => x.applicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShippingMethods_containers_containerId",
                        column: x => x.containerId,
                        principalTable: "containers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingMethods_applicationUserId",
                table: "ShippingMethods",
                column: "applicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingMethods_containerId",
                table: "ShippingMethods",
                column: "containerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingMethods");
        }
    }
}
