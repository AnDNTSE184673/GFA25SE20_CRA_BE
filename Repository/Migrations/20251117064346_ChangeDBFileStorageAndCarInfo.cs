using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDBFileStorageAndCarInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlExpiration",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "UrlExpiration",
                table: "CarRegistrations");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Cars",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Cars",
                newName: "Transmission");

            migrationBuilder.RenameColumn(
                name: "CarType",
                table: "Cars",
                newName: "FuelType");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "DriverLicenses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "DriverLicenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FuelConsumption",
                table: "Cars",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "YearofManufacture",
                table: "Cars",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "CarRegistrations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "CarRegistrations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Bucket = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CarId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarImages_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarRentalRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DailyRate = table.Column<double>(type: "double precision", nullable: false),
                    HourlyRate = table.Column<double>(type: "double precision", nullable: false),
                    WeeklyDiscount = table.Column<double>(type: "double precision", nullable: false),
                    MonthlyDiscount = table.Column<double>(type: "double precision", nullable: false),
                    OvertimeRate = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CarId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarRentalRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarRentalRates_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Bucket = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FeedbackId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackImages_Feedbacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "Feedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarImages_CarId",
                table: "CarImages",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_CarRentalRates_CarId",
                table: "CarRentalRates",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackImages_FeedbackId",
                table: "FeedbackImages",
                column: "FeedbackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarImages");

            migrationBuilder.DropTable(
                name: "CarRentalRates");

            migrationBuilder.DropTable(
                name: "FeedbackImages");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "FuelConsumption",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "YearofManufacture",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "CarRegistrations");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "CarRegistrations");

            migrationBuilder.RenameColumn(
                name: "Transmission",
                table: "Cars",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "FuelType",
                table: "Cars",
                newName: "CarType");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Cars",
                newName: "Notes");

            migrationBuilder.AddColumn<DateTime>(
                name: "UrlExpiration",
                table: "DriverLicenses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "Cars",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UrlExpiration",
                table: "CarRegistrations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
