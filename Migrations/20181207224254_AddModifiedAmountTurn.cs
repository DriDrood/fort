using Microsoft.EntityFrameworkCore.Migrations;

namespace Fort.Migrations
{
    public partial class AddModifiedAmountTurn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModifiedAmount",
                table: "Turns",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedAmount",
                table: "Turns");
        }
    }
}
