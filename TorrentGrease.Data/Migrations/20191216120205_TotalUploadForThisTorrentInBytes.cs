using Microsoft.EntityFrameworkCore.Migrations;

namespace TorrentGrease.Data.Migrations
{
    public partial class TotalUploadForThisTorrentInBytes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TotalUploadForThisTorrentInBytes",
                table: "TorrentUploadDeltaSnapshot",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalUploadForThisTorrentInBytes",
                table: "TorrentUploadDeltaSnapshot");
        }
    }
}
