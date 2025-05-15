using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorName",
                table: "CraneUsageRecords");

            migrationBuilder.AddColumn<string>(
                name: "OperatorName",
                table: "CraneUsageEntries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorName",
                table: "CraneUsageEntries");

            migrationBuilder.AddColumn<string>(
                name: "OperatorName",
                table: "CraneUsageRecords",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
