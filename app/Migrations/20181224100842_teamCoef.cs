using Microsoft.EntityFrameworkCore.Migrations;

namespace Fort.Migrations
{
    public partial class teamCoef : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ArmyStrengthCoef",
                table: "Teams",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AddColumn<int>(
                name: "PopulationGrowth",
                table: "Teams",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArmyStrengthCoef",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "PopulationGrowth",
                table: "Teams");
        }
    }
}
