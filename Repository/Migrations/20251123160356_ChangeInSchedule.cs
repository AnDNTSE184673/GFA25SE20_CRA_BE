using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookingId",
                table: "Schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocking",
                table: "Schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Schedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Schedules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_BookingId",
                table: "Schedules",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_BookingHistories_BookingId",
                table: "Schedules",
                column: "BookingId",
                principalTable: "BookingHistories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_BookingHistories_BookingId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_BookingId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "IsBlocking",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Schedules");
        }
    }
}
