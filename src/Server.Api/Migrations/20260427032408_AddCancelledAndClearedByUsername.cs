using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledAndClearedByUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelledByUsername",
                table: "ManualShelfDeclarations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClearedByUsername",
                table: "ManualShelfDeclarations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledByUsername",
                table: "ManualShelfDeclarations");

            migrationBuilder.DropColumn(
                name: "ClearedByUsername",
                table: "ManualShelfDeclarations");
        }
    }
}
