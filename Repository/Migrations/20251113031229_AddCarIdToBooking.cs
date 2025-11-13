using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCarIdToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CarId",
                table: "BookingHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_BookingHistories_CarId",
                table: "BookingHistories",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingHistories_Cars_CarId",
                table: "BookingHistories",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingHistories_Cars_CarId",
                table: "BookingHistories");

            migrationBuilder.DropIndex(
                name: "IX_BookingHistories_CarId",
                table: "BookingHistories");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "BookingHistories");
        }
    }
}
