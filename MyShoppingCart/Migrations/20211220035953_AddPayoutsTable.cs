using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyShoppingCart.Migrations
{
    public partial class AddPayoutsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "payoutId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Payouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    applicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    payoutAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    payoutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailSubject = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payouts_AspNetUsers_applicationUserId",
                        column: x => x.applicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_payoutId",
                table: "OrderItems",
                column: "payoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Payouts_applicationUserId",
                table: "Payouts",
                column: "applicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Payouts_payoutId",
                table: "OrderItems",
                column: "payoutId",
                principalTable: "Payouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Payouts_payoutId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "Payouts");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_payoutId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "payoutId",
                table: "OrderItems");
        }
    }
}
