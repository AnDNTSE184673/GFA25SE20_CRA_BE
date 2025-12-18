using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOCRFieldsToDriverLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicenseClass",
                table: "DriverLicenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LicenseDoB",
                table: "DriverLicenses",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LicenseExpiry",
                table: "DriverLicenses",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LicenseIssue",
                table: "DriverLicenses",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseName",
                table: "DriverLicenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "DriverLicenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Side",
                table: "DriverLicenses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseClass",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "LicenseDoB",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "LicenseExpiry",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "LicenseIssue",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "LicenseName",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "DriverLicenses");
        }
    }
}
