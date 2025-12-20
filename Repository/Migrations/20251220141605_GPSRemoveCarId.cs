using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class GPSRemoveCarId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GPS_Cars_CarId",
                table: "GPS");

            migrationBuilder.DropIndex(
                name: "IX_GPS_CarId",
                table: "GPS");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "GPS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CarId",
                table: "GPS",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_GPS_CarId",
                table: "GPS",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_GPS_Cars_CarId",
                table: "GPS",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
