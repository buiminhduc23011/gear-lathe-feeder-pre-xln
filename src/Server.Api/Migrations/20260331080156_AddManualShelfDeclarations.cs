using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddManualShelfDeclarations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManualShelfDeclarations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    KeIndex = table.Column<int>(type: "int", nullable: false),
                    ShelfLayoutType = table.Column<int>(type: "int", nullable: false),
                    OrdersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AppliedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualShelfDeclarations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManualShelfDeclarations_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManualShelfDeclarations_MachineId_KeIndex",
                table: "ManualShelfDeclarations",
                columns: new[] { "MachineId", "KeIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualShelfDeclarations_MachineId_Status",
                table: "ManualShelfDeclarations",
                columns: new[] { "MachineId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManualShelfDeclarations");
        }
    }
}
