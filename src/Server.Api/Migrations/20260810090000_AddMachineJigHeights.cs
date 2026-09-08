using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Api.Migrations;

public partial class AddMachineJigHeights : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<float>("Jig1HeightMm", "Machines", type: "real", nullable: false, defaultValue: 0f);
        migrationBuilder.AddColumn<float>("Jig2HeightMm", "Machines", type: "real", nullable: false, defaultValue: 0f);
        migrationBuilder.AddColumn<float>("Jig3HeightMm", "Machines", type: "real", nullable: false, defaultValue: 0f);
        migrationBuilder.AddColumn<float>("Jig4HeightMm", "Machines", type: "real", nullable: false, defaultValue: 0f);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("Jig1HeightMm", "Machines");
        migrationBuilder.DropColumn("Jig2HeightMm", "Machines");
        migrationBuilder.DropColumn("Jig3HeightMm", "Machines");
        migrationBuilder.DropColumn("Jig4HeightMm", "Machines");
    }
}
