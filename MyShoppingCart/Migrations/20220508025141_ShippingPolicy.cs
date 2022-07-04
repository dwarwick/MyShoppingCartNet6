using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShoppingCart.Migrations
{
    public partial class ShippingPolicy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payouts_AspNetUsers_applicationUserId",
                table: "Payouts");

            migrationBuilder.AlterColumn<string>(
                name: "applicationUserId",
                table: "Payouts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ShippingClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ShippingClassId = table.Column<int>(type: "int", nullable: true),
                    Criteria = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingPolicies_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShippingPolicies_ShippingClasses_ShippingClassId",
                        column: x => x.ShippingClassId,
                        principalTable: "ShippingClasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingPolicies_ApplicationUserId",
                table: "ShippingPolicies",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingPolicies_ShippingClassId",
                table: "ShippingPolicies",
                column: "ShippingClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payouts_AspNetUsers_applicationUserId",
                table: "Payouts",
                column: "applicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payouts_AspNetUsers_applicationUserId",
                table: "Payouts");

            migrationBuilder.DropTable(
                name: "ShippingPolicies");

            migrationBuilder.DropTable(
                name: "ShippingClasses");

            migrationBuilder.AlterColumn<string>(
                name: "applicationUserId",
                table: "Payouts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Payouts_AspNetUsers_applicationUserId",
                table: "Payouts",
                column: "applicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
