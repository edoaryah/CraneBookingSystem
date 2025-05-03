using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class CraneUsageRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CraneUsageRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    SubcategoryId = table.Column<int>(type: "integer", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CraneUsageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CraneUsageRecords_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsageSubcategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageSubcategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UsageSubcategories",
                columns: new[] { "Id", "Category", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Operating", "Crane used for production lifting operations", true, "Production Lifting" },
                    { 2, "Operating", "Crane used for equipment installation", true, "Equipment Installation" },
                    { 3, "Operating", "Crane used for general material handling", true, "Material Handling" },
                    { 4, "Delay", "Delay due to bad weather conditions", true, "Weather" },
                    { 5, "Delay", "Delay due to planning or coordination issues", true, "Planning" },
                    { 6, "Delay", "Scheduled operator break time", true, "Operator Break" },
                    { 7, "Delay", "Delay for refueling operations", true, "Refueling" },
                    { 8, "Standby", "Crane on standby at work site", true, "On-site Standby" },
                    { 9, "Standby", "Crane available but not assigned any tasks", true, "No Work Assignment" },
                    { 10, "Standby", "Planned standby period", true, "Scheduled Standby" },
                    { 11, "Service", "Regular scheduled maintenance", true, "Scheduled Maintenance" },
                    { 12, "Service", "Safety or regulatory inspection", true, "Inspection" },
                    { 13, "Service", "Planned replacement of components", true, "Component Replacement" },
                    { 14, "Service", "Regular lubrication service", true, "Lubrication" },
                    { 15, "Breakdown", "Breakdown due to mechanical problems", true, "Mechanical Failure" },
                    { 16, "Breakdown", "Breakdown due to electrical problems", true, "Electrical Failure" },
                    { 17, "Breakdown", "Breakdown due to hydraulic system problems", true, "Hydraulic Failure" },
                    { 18, "Breakdown", "Breakdown due to control system issues", true, "Control System Failure" },
                    { 19, "Breakdown", "Breakdown due to accident or incident", true, "Accident" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CraneUsageRecords_BookingId",
                table: "CraneUsageRecords",
                column: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CraneUsageRecords");

            migrationBuilder.DropTable(
                name: "UsageSubcategories");
        }
    }
}
