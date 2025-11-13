using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseFileStorageChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "DriverLicenses",
                newName: "UrlExpiration");

            migrationBuilder.RenameColumn(
                name: "DocUrl",
                table: "DriverLicenses",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "CarRegistrations",
                newName: "UrlExpiration");

            migrationBuilder.RenameColumn(
                name: "DocUrl",
                table: "CarRegistrations",
                newName: "FilePath");

            migrationBuilder.AddColumn<string>(
                name: "Bucket",
                table: "DriverLicenses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Bucket",
                table: "CarRegistrations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bucket",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "Bucket",
                table: "CarRegistrations");

            migrationBuilder.RenameColumn(
                name: "UrlExpiration",
                table: "DriverLicenses",
                newName: "UpdateDate");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "DriverLicenses",
                newName: "DocUrl");

            migrationBuilder.RenameColumn(
                name: "UrlExpiration",
                table: "CarRegistrations",
                newName: "UpdateDate");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "CarRegistrations",
                newName: "DocUrl");
        }
    }
}
