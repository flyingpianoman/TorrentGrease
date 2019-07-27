using Microsoft.EntityFrameworkCore.Migrations;

namespace TorrentGrease.Data.Migrations
{
    public partial class UniqueConstraintsOnOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Condition_Order",
                table: "Condition",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Action_Order",
                table: "Action",
                column: "Order",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Condition_Order",
                table: "Condition");

            migrationBuilder.DropIndex(
                name: "IX_Action_Order",
                table: "Action");
        }
    }
}
