using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TorrentGrease.Data.Migrations
{
    public partial class InitialTorrentStatisticsSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Torrent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InfoHash = table.Column<string>(nullable: true),
                    WasInClientOnLastScan = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    LatestAddedDateTime = table.Column<DateTime>(nullable: false),
                    SizeInBytes = table.Column<long>(nullable: false),
                    BytesOnDisk = table.Column<long>(nullable: false),
                    TotalUploadInBytes = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Torrent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackerUrlCollection",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionHash = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerUrlCollection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TorrentUploadDeltaSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    TorrentId = table.Column<int>(nullable: false),
                    TotalUploadForThisTorrentInBytes = table.Column<long>(nullable: false),
                    UploadDeltaSinceLastSnapshotInBytes = table.Column<long>(nullable: false),
                    TotalUploadInBytes = table.Column<long>(nullable: false),
                    TrackerUrlCollectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorrentUploadDeltaSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TorrentUploadDeltaSnapshot_Torrent_TorrentId",
                        column: x => x.TorrentId,
                        principalTable: "Torrent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TorrentUploadDeltaSnapshot_TrackerUrlCollection_TrackerUrlCollectionId",
                        column: x => x.TrackerUrlCollectionId,
                        principalTable: "TrackerUrlCollection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackerUrl",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackerUrlCollectionId = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerUrl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackerUrl_TrackerUrlCollection_TrackerUrlCollectionId",
                        column: x => x.TrackerUrlCollectionId,
                        principalTable: "TrackerUrlCollection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Torrent_InfoHash",
                table: "Torrent",
                column: "InfoHash");

            migrationBuilder.CreateIndex(
                name: "IX_Torrent_WasInClientOnLastScan",
                table: "Torrent",
                column: "WasInClientOnLastScan");

            migrationBuilder.CreateIndex(
                name: "IX_TorrentUploadDeltaSnapshot_TrackerUrlCollectionId",
                table: "TorrentUploadDeltaSnapshot",
                column: "TrackerUrlCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TorrentUploadDeltaSnapshot_TorrentId_DateTime",
                table: "TorrentUploadDeltaSnapshot",
                columns: new[] { "TorrentId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackerUrl_TrackerUrlCollectionId",
                table: "TrackerUrl",
                column: "TrackerUrlCollectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TorrentUploadDeltaSnapshot");

            migrationBuilder.DropTable(
                name: "TrackerUrl");

            migrationBuilder.DropTable(
                name: "Torrent");

            migrationBuilder.DropTable(
                name: "TrackerUrlCollection");
        }
    }
}
