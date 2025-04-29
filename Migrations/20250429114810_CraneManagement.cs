using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class CraneManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cranes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Ownership = table.Column<string>(type: "text", nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cranes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Breakdowns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CraneId = table.Column<int>(type: "integer", nullable: false),
                    UrgentStartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UrgentEndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ActualUrgentEndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    HangfireJobId = table.Column<string>(type: "text", nullable: true),
                    Reasons = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Breakdowns_Cranes_CraneId",
                        column: x => x.CraneId,
                        principalTable: "Cranes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "LdapUser", "Notes", "RoleName", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 29, 19, 48, 10, 448, DateTimeKind.Local).AddTicks(7690), "system", "PIC1", "Default admin user created by seeder", "admin", null, null },
                    { 2, new DateTime(2025, 4, 29, 19, 48, 10, 448, DateTimeKind.Local).AddTicks(7980), "system", "PIC1", "Default admin user created by seeder", "pic", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Breakdowns_CraneId",
                table: "Breakdowns",
                column: "CraneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Breakdowns");

            migrationBuilder.DropTable(
                name: "Cranes");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
