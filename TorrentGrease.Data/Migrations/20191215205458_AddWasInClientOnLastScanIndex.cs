using Microsoft.EntityFrameworkCore.Migrations;

namespace TorrentGrease.Data.Migrations
{
    public partial class AddWasInClientOnLastScanIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Torrent_WasInClientOnLastScan",
                table: "Torrent",
                column: "WasInClientOnLastScan");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Torrent_WasInClientOnLastScan",
                table: "Torrent");
        }
    }
}
