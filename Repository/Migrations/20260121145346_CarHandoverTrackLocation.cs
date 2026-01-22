using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class CarHandoverTrackLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarHandoverAudits_Schedules_ScheduleId",
                table: "CarHandoverAudits");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleId",
                table: "CarHandoverAudits",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "CarHandoverAudits",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_CarHandoverAudits_Schedules_ScheduleId",
                table: "CarHandoverAudits",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarHandoverAudits_Schedules_ScheduleId",
                table: "CarHandoverAudits");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "CarHandoverAudits");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleId",
                table: "CarHandoverAudits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CarHandoverAudits_Schedules_ScheduleId",
                table: "CarHandoverAudits",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
