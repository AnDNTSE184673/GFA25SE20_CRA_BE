using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class TollBoothAndCarTravelLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TollBooths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    ChargeAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollBooths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarTravelLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TravelDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TollBoothId = table.Column<int>(type: "integer", nullable: false),
                    ChargeAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarTravelLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarTravelLogs_BookingHistories_BookingId",
                        column: x => x.BookingId,
                        principalTable: "BookingHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarTravelLogs_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarTravelLogs_TollBooths_TollBoothId",
                        column: x => x.TollBoothId,
                        principalTable: "TollBooths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TollBooths",
                columns: new[] { "Id", "ChargeAmount", "Location", "Name" },
                values: new object[,]
                {
                    { 1, 100000m, "249 Võ Nguyên Giáp, Phước Long A, Thủ Đức, Thành phố Hồ Chí Minh, Vietnam", "Trạm thu phí xa lộ Hà Nội" },
                    { 2, 150000m, "Trạm thu phí Long Phước, Long Phước, Thủ Đức, Thành phố Hồ Chí Minh, Vietnam", "Trạm Thu phí Long Phước" },
                    { 3, 120000m, "WP27+8P6, ĐT743B, Binh Hoà, Thuận An, Bình Dương, Vietnam", "Trạm Thu Phí Cầu Ông Bố" },
                    { 4, 200000m, "702 Đường Nguyễn Văn Linh, Tân Hưng, Quận 7, Thành phố Hồ Chí Minh, Vietnam", "Trạm Thu phí Nguyễn Văn Linh" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarTravelLogs_BookingId",
                table: "CarTravelLogs",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarTravelLogs_CarId",
                table: "CarTravelLogs",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_CarTravelLogs_TollBoothId",
                table: "CarTravelLogs",
                column: "TollBoothId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarTravelLogs");

            migrationBuilder.DropTable(
                name: "TollBooths");
        }
    }
}
