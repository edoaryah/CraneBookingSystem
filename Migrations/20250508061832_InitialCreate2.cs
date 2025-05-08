using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CraneUsageRecords_Bookings_BookingId",
                table: "CraneUsageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_CraneUsageRecords_MaintenanceSchedules_MaintenanceScheduleId",
                table: "CraneUsageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_CraneUsageRecords_UsageSubcategories_UsageSubcategoryId",
                table: "CraneUsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_CraneUsageRecords_BookingId",
                table: "CraneUsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_CraneUsageRecords_MaintenanceScheduleId",
                table: "CraneUsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_CraneUsageRecords_UsageSubcategoryId",
                table: "CraneUsageRecords");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "CraneUsageRecords");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "CraneUsageRecords");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "CraneUsageRecords");

            migrationBuilder.DropColumn(
                name: "MaintenanceScheduleId",
                table: "CraneUsageRecords");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CraneUsageRecords");

            migrationBuilder.DropColumn(
                name: "UsageSubcategoryId",
                table: "CraneUsageRecords");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "CraneUsageRecords",
                newName: "Date");

            migrationBuilder.CreateTable(
                name: "CraneUsageEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CraneUsageRecordId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    UsageSubcategoryId = table.Column<int>(type: "integer", nullable: false),
                    BookingId = table.Column<int>(type: "integer", nullable: true),
                    MaintenanceScheduleId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CraneUsageEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CraneUsageEntries_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CraneUsageEntries_CraneUsageRecords_CraneUsageRecordId",
                        column: x => x.CraneUsageRecordId,
                        principalTable: "CraneUsageRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CraneUsageEntries_MaintenanceSchedules_MaintenanceScheduleId",
                        column: x => x.MaintenanceScheduleId,
                        principalTable: "MaintenanceSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CraneUsageEntries_UsageSubcategories_UsageSubcategoryId",
                        column: x => x.UsageSubcategoryId,
                        principalTable: "UsageSubcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CraneUsageEntries_BookingId",
                table: "CraneUsageEntries",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CraneUsageEntries_CraneUsageRecordId",
                table: "CraneUsageEntries",
                column: "CraneUsageRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CraneUsageEntries_MaintenanceScheduleId",
                table: "CraneUsageEntries",
                column: "MaintenanceScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_CraneUsageEntries_UsageSubcategoryId",
                table: "CraneUsageEntries",
                column: "UsageSubcategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CraneUsageEntries");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "CraneUsageRecords",
                newName: "StartTime");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "CraneUsageRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "CraneUsageRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "CraneUsageRecords",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaintenanceScheduleId",
                table: "CraneUsageRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CraneUsageRecords",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsageSubcategoryId",
                table: "CraneUsageRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CraneUsageRecords_BookingId",
                table: "CraneUsageRecords",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CraneUsageRecords_MaintenanceScheduleId",
                table: "CraneUsageRecords",
                column: "MaintenanceScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_CraneUsageRecords_UsageSubcategoryId",
                table: "CraneUsageRecords",
                column: "UsageSubcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CraneUsageRecords_Bookings_BookingId",
                table: "CraneUsageRecords",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CraneUsageRecords_MaintenanceSchedules_MaintenanceScheduleId",
                table: "CraneUsageRecords",
                column: "MaintenanceScheduleId",
                principalTable: "MaintenanceSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CraneUsageRecords_UsageSubcategories_UsageSubcategoryId",
                table: "CraneUsageRecords",
                column: "UsageSubcategoryId",
                principalTable: "UsageSubcategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
