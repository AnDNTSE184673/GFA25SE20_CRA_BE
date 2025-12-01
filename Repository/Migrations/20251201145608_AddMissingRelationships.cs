using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CarRentalRates_CarId",
                table: "CarRentalRates");

            migrationBuilder.CreateIndex(
                name: "IX_OTPCodes_UserId",
                table: "OTPCodes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CarRentalRates_CarId",
                table: "CarRentalRates",
                column: "CarId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OTPCodes_Users_UserId",
                table: "OTPCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OTPCodes_Users_UserId",
                table: "OTPCodes");

            migrationBuilder.DropIndex(
                name: "IX_OTPCodes_UserId",
                table: "OTPCodes");

            migrationBuilder.DropIndex(
                name: "IX_CarRentalRates_CarId",
                table: "CarRentalRates");

            migrationBuilder.CreateIndex(
                name: "IX_CarRentalRates_CarId",
                table: "CarRentalRates",
                column: "CarId");
        }
    }
}
