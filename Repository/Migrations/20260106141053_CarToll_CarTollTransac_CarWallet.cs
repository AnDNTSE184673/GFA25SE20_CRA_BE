using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class CarToll_CarTollTransac_CarWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarTolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingNum = table.Column<string>(type: "text", nullable: false),
                    CarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarTolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarTolls_BookingHistories_BookingId",
                        column: x => x.BookingId,
                        principalTable: "BookingHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarTolls_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarWallets_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarTollTransacs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransacDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CarTollId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarTollTransacs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarTollTransacs_CarTolls_CarTollId",
                        column: x => x.CarTollId,
                        principalTable: "CarTolls",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarTolls_BookingId",
                table: "CarTolls",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarTolls_CarId",
                table: "CarTolls",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_CarTollTransacs_CarTollId",
                table: "CarTollTransacs",
                column: "CarTollId");

            migrationBuilder.CreateIndex(
                name: "IX_CarWallets_CarId",
                table: "CarWallets",
                column: "CarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarTollTransacs");

            migrationBuilder.DropTable(
                name: "CarWallets");

            migrationBuilder.DropTable(
                name: "CarTolls");
        }
    }
}
