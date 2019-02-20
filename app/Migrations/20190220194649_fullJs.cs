using Microsoft.EntityFrameworkCore.Migrations;

namespace Fort.Migrations
{
    public partial class fullJs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastRoundReady",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Teams",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRoundReady",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Teams");
        }
    }
}
