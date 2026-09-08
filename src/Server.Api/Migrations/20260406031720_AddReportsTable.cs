using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ManualShelfDeclarations_MachineId_KeIndex' AND object_id = OBJECT_ID('ManualShelfDeclarations')) " +
                "DROP INDEX [IX_ManualShelfDeclarations_MachineId_KeIndex] ON [ManualShelfDeclarations];");

            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ManualShelfDeclarations_MachineId_Status' AND object_id = OBJECT_ID('ManualShelfDeclarations')) " +
                "DROP INDEX [IX_ManualShelfDeclarations_MachineId_Status] ON [ManualShelfDeclarations];");

            migrationBuilder.DropColumn(
                name: "KeIndex",
                table: "ManualShelfDeclarations");

            migrationBuilder.RenameColumn(
                name: "AppliedAtUtc",
                table: "ManualShelfDeclarations",
                newName: "ProductionStartedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ManualShelfDeclarations",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AgvTakenAtUtc",
                table: "ManualShelfDeclarations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAtUtc",
                table: "ManualShelfDeclarations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClearedAtUtc",
                table: "ManualShelfDeclarations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAtUtc",
                table: "ManualShelfDeclarations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LoadRequestedAtUtc",
                table: "ManualShelfDeclarations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LoadedAtUtc",
                table: "ManualShelfDeclarations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineCodeSnapshot",
                table: "ManualShelfDeclarations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MachineNameSnapshot",
                table: "ManualShelfDeclarations",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MachineSlotIndex",
                table: "ManualShelfDeclarations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mode",
                table: "ManualShelfDeclarations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PickedByAgvId",
                table: "ManualShelfDeclarations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickedByAgvName",
                table: "ManualShelfDeclarations",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionDurationSeconds",
                table: "ManualShelfDeclarations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StagingSlotIndex",
                table: "ManualShelfDeclarations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                table: "ManualShelfDeclarations",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "AssignedStagingSlot1",
                table: "Machines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedStagingSlot2",
                table: "Machines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalJobs = table.Column<int>(type: "int", nullable: false),
                    PassedJobs = table.Column<int>(type: "int", nullable: false),
                    FailedJobs = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShelfDeclarationEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeclarationId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EventAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    MachineSlotIndex = table.Column<int>(type: "int", nullable: true),
                    StagingSlotIndex = table.Column<int>(type: "int", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelfDeclarationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShelfDeclarationEvents_ManualShelfDeclarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "ManualShelfDeclarations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManualShelfDeclarations_MachineId_MachineSlotIndex",
                table: "ManualShelfDeclarations",
                columns: new[] { "MachineId", "MachineSlotIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualShelfDeclarations_MachineId_Mode_Status",
                table: "ManualShelfDeclarations",
                columns: new[] { "MachineId", "Mode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualShelfDeclarations_MachineId_StagingSlotIndex",
                table: "ManualShelfDeclarations",
                columns: new[] { "MachineId", "StagingSlotIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_MachineCode_ReportDate",
                table: "Reports",
                columns: new[] { "MachineCode", "ReportDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShelfDeclarationEvents_DeclarationId_EventAtUtc",
                table: "ShelfDeclarationEvents",
                columns: new[] { "DeclarationId", "EventAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ShelfDeclarationEvents");

            migrationBuilder.DropIndex(
                name: "IX_ManualShelfDeclarations_MachineId_MachineSlotIndex",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropIndex(
                name: "IX_ManualShelfDeclarations_MachineId_Mode_Status",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropIndex(
                name: "IX_ManualShelfDeclarations_MachineId_StagingSlotIndex",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "AgvTakenAtUtc",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "ClearedAtUtc",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "LoadRequestedAtUtc",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "LoadedAtUtc",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "MachineCodeSnapshot",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "MachineNameSnapshot",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "MachineSlotIndex",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "PickedByAgvId",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "PickedByAgvName",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "ProductionDurationSeconds",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "StagingSlotIndex",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "AssignedStagingSlot1",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "AssignedStagingSlot2",
                table: "Machines");

            migrationBuilder.RenameColumn(
                name: "ProductionStartedAtUtc",
                table: "ManualShelfDeclarations",
                newName: "AppliedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ManualShelfDeclarations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<int>(
                name: "KeIndex",
                table: "ManualShelfDeclarations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ManualShelfDeclarations_MachineId_KeIndex",
                table: "ManualShelfDeclarations",
                columns: new[] { "MachineId", "KeIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualShelfDeclarations_MachineId_Status",
                table: "ManualShelfDeclarations",
                columns: new[] { "MachineId", "Status" });
        }
    }
}
