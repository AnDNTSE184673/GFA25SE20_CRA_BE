using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OvertimeRate",
                table: "CarRentalRates",
                newName: "OvertravelRatePerKm");

            migrationBuilder.AddColumn<string>(
                name: "TermsDetail",
                table: "Contracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxDistancePerDay",
                table: "CarRentalRates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarHandoverAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    VerificationMethod = table.Column<string>(type: "text", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibleStaffId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarHandoverAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarHandoverAudits_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarHandoverAudits_Users_ResponsibleStaffId",
                        column: x => x.ResponsibleStaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OTPCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OtpHash = table.Column<string>(type: "text", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaffLogAudit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RelatedHandoverId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffLogAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffLogAudit_CarHandoverAudits_RelatedHandoverId",
                        column: x => x.RelatedHandoverId,
                        principalTable: "CarHandoverAudits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffLogAudit_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarHandoverAudits_ResponsibleStaffId",
                table: "CarHandoverAudits",
                column: "ResponsibleStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_CarHandoverAudits_ScheduleId",
                table: "CarHandoverAudits",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLogAudit_RelatedHandoverId",
                table: "StaffLogAudit",
                column: "RelatedHandoverId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLogAudit_StaffId",
                table: "StaffLogAudit",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OTPCodes");

            migrationBuilder.DropTable(
                name: "StaffLogAudit");

            migrationBuilder.DropTable(
                name: "CarHandoverAudits");

            migrationBuilder.DropColumn(
                name: "TermsDetail",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "MaxDistancePerDay",
                table: "CarRentalRates");

            migrationBuilder.RenameColumn(
                name: "OvertravelRatePerKm",
                table: "CarRentalRates",
                newName: "OvertimeRate");
        }
    }
}
