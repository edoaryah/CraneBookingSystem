using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                column: "EndTime",
                value: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                column: "StartTime",
                value: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                column: "EndTime",
                value: new TimeSpan(0, 23, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "ShiftDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                column: "StartTime",
                value: new TimeSpan(0, 23, 0, 0, 0));
        }
    }
}
