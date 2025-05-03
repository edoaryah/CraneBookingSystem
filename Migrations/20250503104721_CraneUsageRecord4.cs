using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class CraneUsageRecord4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "CraneUsageRecords",
                newName: "StartTime");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "CraneUsageRecords",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "CraneUsageRecords");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "CraneUsageRecords",
                newName: "Duration");
        }
    }
}
