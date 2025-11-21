using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleAndBookingChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookingHistories_InvoiceId",
                table: "BookingHistories");

            migrationBuilder.DropColumn(
                name: "LotId",
                table: "Cars");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "PaymentTransactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransactionStatus",
                table: "PaymentTransactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "OrderCode",
                table: "PaymentHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "PaymentHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "PaymentHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "WeeklyDiscount",
                table: "CarRentalRates",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "OvertimeRate",
                table: "CarRentalRates",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "MonthlyDiscount",
                table: "CarRentalRates",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "HourlyRate",
                table: "CarRentalRates",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "DailyRate",
                table: "CarRentalRates",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<string>(
                name: "DropoffPlace",
                table: "BookingHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DropoffTime",
                table: "BookingHistories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PickupPlace",
                table: "BookingHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupTime",
                table: "BookingHistories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_UserId",
                table: "Schedules",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingHistories_InvoiceId",
                table: "BookingHistories",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_UserId",
                table: "Schedules",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_UserId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_UserId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_BookingHistories_InvoiceId",
                table: "BookingHistories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionStatus",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "DropoffPlace",
                table: "BookingHistories");

            migrationBuilder.DropColumn(
                name: "DropoffTime",
                table: "BookingHistories");

            migrationBuilder.DropColumn(
                name: "PickupPlace",
                table: "BookingHistories");

            migrationBuilder.DropColumn(
                name: "PickupTime",
                table: "BookingHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "LotId",
                table: "Cars",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<double>(
                name: "WeeklyDiscount",
                table: "CarRentalRates",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "OvertimeRate",
                table: "CarRentalRates",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MonthlyDiscount",
                table: "CarRentalRates",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "HourlyRate",
                table: "CarRentalRates",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "DailyRate",
                table: "CarRentalRates",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingHistories_InvoiceId",
                table: "BookingHistories",
                column: "InvoiceId");
        }
    }
}
