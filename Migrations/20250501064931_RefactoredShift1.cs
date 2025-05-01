using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class RefactoredShift1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "ShiftDefinitions");

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndTime", "Name", "StartTime" },
                values: new object[] { new TimeSpan(0, 15, 0, 0, 0), "Morning Shift", new TimeSpan(0, 7, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EndTime", "Name", "StartTime" },
                values: new object[] { new TimeSpan(0, 23, 0, 0, 0), "Evening Shift", new TimeSpan(0, 15, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EndTime", "Name", "StartTime" },
                values: new object[] { new TimeSpan(0, 7, 0, 0, 0), "Night Shift", new TimeSpan(0, 23, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EndTime", "Name", "StartTime" },
                values: new object[] { new TimeSpan(0, 11, 0, 0, 0), "First Half Day", new TimeSpan(0, 7, 0, 0, 0) });

            migrationBuilder.InsertData(
                table: "ShiftDefinitions",
                columns: new[] { "Id", "EndTime", "IsActive", "Name", "StartTime" },
                values: new object[] { 5, new TimeSpan(0, 17, 0, 0, 0), true, "Second Half Day", new TimeSpan(0, 13, 0, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ShiftDefinitions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "EndTime", "Name", "StartTime" },
                values: new object[] { "Day Shift", new TimeSpan(0, 12, 0, 0, 0), "Pagi (06:00-12:00)", new TimeSpan(0, 6, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "EndTime", "Name", "StartTime" },
                values: new object[] { "Day Shift", new TimeSpan(0, 18, 0, 0, 0), "Siang (12:00-18:00)", new TimeSpan(0, 12, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "EndTime", "Name", "StartTime" },
                values: new object[] { "Night Shift", new TimeSpan(0, 0, 0, 0, 0), "Sore (18:00-00:00)", new TimeSpan(0, 18, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "EndTime", "Name", "StartTime" },
                values: new object[] { "Night Shift", new TimeSpan(0, 6, 0, 0, 0), "Malam (00:00-06:00)", new TimeSpan(0, 0, 0, 0, 0) });
        }
    }
}
