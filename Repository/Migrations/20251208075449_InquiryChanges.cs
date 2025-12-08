using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class InquiryChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentInquiryId",
                table: "Inquiries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Inquiries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InquiryImages",
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
                    InquiryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquiryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InquiryImages_Inquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_ParentInquiryId",
                table: "Inquiries",
                column: "ParentInquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_InquiryImages_InquiryId",
                table: "InquiryImages",
                column: "InquiryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_Inquiries_ParentInquiryId",
                table: "Inquiries",
                column: "ParentInquiryId",
                principalTable: "Inquiries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_Inquiries_ParentInquiryId",
                table: "Inquiries");

            migrationBuilder.DropTable(
                name: "InquiryImages");

            migrationBuilder.DropIndex(
                name: "IX_Inquiries_ParentInquiryId",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "ParentInquiryId",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Inquiries");
        }
    }
}
